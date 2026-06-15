using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class RefreshAccessTokenTests
{
    private readonly IHashService _hashService;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ISessionRepository _sessionRepository;

    private readonly RefreshAccessToken.Handler _handler;

    public RefreshAccessTokenTests()
    {
        _hashService = Substitute.For<IHashService>();
        _accessTokenService = Substitute.For<IAccessTokenService>();
        _refreshTokenService = Substitute.For<IRefreshTokenService>();
        _sessionRepository = Substitute.For<ISessionRepository>();

        _handler = new RefreshAccessToken.Handler(
            _hashService,
            _accessTokenService,
            _refreshTokenService,
            _sessionRepository);
    }

    [Fact]
    public async Task Handle_CurrentRefreshToken_StoresPendingTokensAndReturnsNewTokens()
    {
        var command = new RefreshAccessToken.Command("mock-currentRefreshToken!1");
        var currentRefreshTokenHash = "hash-currentRefreshToken@2"u8.ToArray();
        var newAccessTokenHash = "hash-newAccessToken#3"u8.ToArray();
        var newRefreshTokenHash = "hash-newRefreshToken$4"u8.ToArray();
        var session = new Session(
            SessionId.Random(),
            UserId.Random(),
            "hash-currentAccessToken%5"u8.ToArray(),
            currentRefreshTokenHash);

        _hashService.HashAsync("mock-currentRefreshToken!1", Arg.Any<CancellationToken>()).Returns(currentRefreshTokenHash);
        _sessionRepository.GetByRefreshTokenAsync(currentRefreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _accessTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newAccessToken^6");
        _refreshTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newRefreshToken&7");
        _hashService.HashAsync("mock-newAccessToken^6", Arg.Any<CancellationToken>()).Returns(newAccessTokenHash);
        _hashService.HashAsync("mock-newRefreshToken&7", Arg.Any<CancellationToken>()).Returns(newRefreshTokenHash);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("mock-newAccessToken^6");
        result.RefreshToken.Should().Be("mock-newRefreshToken&7");
        await _sessionRepository.DidNotReceive().PromotePendingTokensAsync(
            Arg.Any<SessionId>(),
            Arg.Any<byte[]>(),
            Arg.Any<byte[]>(),
            Arg.Any<CancellationToken>());
        await _sessionRepository.Received().SetPendingTokensAsync(
            session.Id,
            newAccessTokenHash,
            newRefreshTokenHash,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PendingRefreshToken_PromotesPendingTokensThenStoresNewPendingTokens()
    {
        var command = new RefreshAccessToken.Command("mock-pendingRefreshToken!1");
        var pendingRefreshTokenHash = "hash-pendingRefreshToken@2"u8.ToArray();
        var newAccessTokenHash = "hash-newAccessToken#3"u8.ToArray();
        var newRefreshTokenHash = "hash-newRefreshToken$4"u8.ToArray();
        var session = new Session(
            SessionId.Random(),
            UserId.Random(),
            "hash-currentAccessToken%5"u8.ToArray(),
            "hash-currentRefreshToken^6"u8.ToArray(),
            "hash-pendingAccessToken&7"u8.ToArray(),
            pendingRefreshTokenHash);

        _hashService.HashAsync("mock-pendingRefreshToken!1", Arg.Any<CancellationToken>()).Returns(pendingRefreshTokenHash);
        _sessionRepository.GetByRefreshTokenAsync(pendingRefreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _sessionRepository.PromotePendingTokensAsync(
            session.Id,
            session.PendingAccessTokenHash!,
            pendingRefreshTokenHash,
            Arg.Any<CancellationToken>()).Returns(true);
        _accessTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newAccessToken*8");
        _refreshTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newRefreshToken(9");
        _hashService.HashAsync("mock-newAccessToken*8", Arg.Any<CancellationToken>()).Returns(newAccessTokenHash);
        _hashService.HashAsync("mock-newRefreshToken(9", Arg.Any<CancellationToken>()).Returns(newRefreshTokenHash);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("mock-newAccessToken*8");
        result.RefreshToken.Should().Be("mock-newRefreshToken(9");
        await _sessionRepository.Received().PromotePendingTokensAsync(
            session.Id,
            session.PendingAccessTokenHash!,
            pendingRefreshTokenHash,
            Arg.Any<CancellationToken>());
        await _sessionRepository.Received().SetPendingTokensAsync(
            session.Id,
            newAccessTokenHash,
            newRefreshTokenHash,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PendingRefreshTokenWhenPromotionFails_ThrowsUnknownTokenException()
    {
        var command = new RefreshAccessToken.Command("mock-pendingRefreshToken!1");
        var pendingRefreshTokenHash = "hash-pendingRefreshToken@2"u8.ToArray();
        var session = new Session(
            SessionId.Random(),
            UserId.Random(),
            "hash-currentAccessToken#3"u8.ToArray(),
            "hash-currentRefreshToken$4"u8.ToArray(),
            "hash-pendingAccessToken%5"u8.ToArray(),
            pendingRefreshTokenHash);

        _hashService.HashAsync("mock-pendingRefreshToken!1", Arg.Any<CancellationToken>()).Returns(pendingRefreshTokenHash);
        _sessionRepository.GetByRefreshTokenAsync(pendingRefreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _sessionRepository.PromotePendingTokensAsync(
            session.Id,
            session.PendingAccessTokenHash!,
            pendingRefreshTokenHash,
            Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnknownTokenException>();
        await _accessTokenService.DidNotReceive().CreateAsync(Arg.Any<CancellationToken>());
        await _refreshTokenService.DidNotReceive().CreateAsync(Arg.Any<CancellationToken>());
        await _sessionRepository.DidNotReceive().SetPendingTokensAsync(
            Arg.Any<SessionId>(),
            Arg.Any<byte[]>(),
            Arg.Any<byte[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownRefreshToken_ThrowsUnknownTokenException()
    {
        var command = new RefreshAccessToken.Command("mock-unknownRefreshToken!1");
        var refreshTokenHash = "hash-unknownRefreshToken@2"u8.ToArray();

        _hashService.HashAsync("mock-unknownRefreshToken!1", Arg.Any<CancellationToken>()).Returns(refreshTokenHash);
        _sessionRepository.GetByRefreshTokenAsync(refreshTokenHash, Arg.Any<CancellationToken>()).Returns((Session?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnknownTokenException>();
        await _accessTokenService.DidNotReceive().CreateAsync(Arg.Any<CancellationToken>());
        await _refreshTokenService.DidNotReceive().CreateAsync(Arg.Any<CancellationToken>());
        await _sessionRepository.DidNotReceive().SetPendingTokensAsync(
            Arg.Any<SessionId>(),
            Arg.Any<byte[]>(),
            Arg.Any<byte[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CurrentRefreshTokenWithExistingPendingTokens_LeavesExistingPendingTokensInPlace()
    {
        var command = new RefreshAccessToken.Command("mock-currentRefreshToken!1");
        var currentRefreshTokenHash = "hash-currentRefreshToken@2"u8.ToArray();
        var newAccessTokenHash = "hash-newAccessToken#3"u8.ToArray();
        var newRefreshTokenHash = "hash-newRefreshToken$4"u8.ToArray();
        var session = new Session(
            SessionId.Random(),
            UserId.Random(),
            "hash-currentAccessToken%5"u8.ToArray(),
            currentRefreshTokenHash,
            null,
            null);

        _hashService.HashAsync("mock-currentRefreshToken!1", Arg.Any<CancellationToken>()).Returns(currentRefreshTokenHash);
        _sessionRepository.GetByRefreshTokenAsync(currentRefreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _accessTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newAccessToken^6");
        _refreshTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newRefreshToken&7");
        _hashService.HashAsync("mock-newAccessToken^6", Arg.Any<CancellationToken>()).Returns(newAccessTokenHash);
        _hashService.HashAsync("mock-newRefreshToken&7", Arg.Any<CancellationToken>()).Returns(newRefreshTokenHash);

        await _handler.Handle(command, CancellationToken.None);

        await _sessionRepository.DidNotReceive().PromotePendingTokensAsync(
            Arg.Any<SessionId>(),
            Arg.Any<byte[]>(),
            Arg.Any<byte[]>(),
            Arg.Any<CancellationToken>());
        await _sessionRepository.Received().SetPendingTokensAsync(
            session.Id,
            newAccessTokenHash,
            newRefreshTokenHash,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var command = new RefreshAccessToken.Command("mock-refreshToken!1");
        var refreshTokenHash = "hash-refreshToken@2"u8.ToArray();
        var accessTokenHash = "hash-accessToken#3"u8.ToArray();
        var newRefreshTokenHash = "hash-newRefreshToken$4"u8.ToArray();
        var session = new Session(
            SessionId.Random(),
            UserId.Random(),
            "hash-currentAccessToken%5"u8.ToArray(),
            refreshTokenHash);

        _hashService.HashAsync("mock-refreshToken!1", Arg.Any<CancellationToken>()).Returns(refreshTokenHash);
        _sessionRepository.GetByRefreshTokenAsync(refreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _accessTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-accessToken^6");
        _refreshTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newRefreshToken&7");
        _hashService.HashAsync("mock-accessToken^6", Arg.Any<CancellationToken>()).Returns(accessTokenHash);
        _hashService.HashAsync("mock-newRefreshToken&7", Arg.Any<CancellationToken>()).Returns(newRefreshTokenHash);

        await _handler.Handle(command, cancellationSource.Token);

        await _hashService.Received().HashAsync("mock-refreshToken!1", cancellationSource.Token);
        await _sessionRepository.Received().GetByRefreshTokenAsync(refreshTokenHash, cancellationSource.Token);
        await _accessTokenService.Received().CreateAsync(cancellationSource.Token);
        await _refreshTokenService.Received().CreateAsync(cancellationSource.Token);
        await _hashService.Received().HashAsync("mock-accessToken^6", cancellationSource.Token);
        await _hashService.Received().HashAsync("mock-newRefreshToken&7", cancellationSource.Token);
        await _sessionRepository.Received().SetPendingTokensAsync(
            session.Id,
            accessTokenHash,
            newRefreshTokenHash,
            cancellationSource.Token);
    }
}