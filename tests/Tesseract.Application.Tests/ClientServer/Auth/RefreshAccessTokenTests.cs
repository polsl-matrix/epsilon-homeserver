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
    private readonly ISessionRepository _sessionRepository;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    private readonly RefreshAccessToken.Handler _handler;

    public RefreshAccessTokenTests()
    {
        _hashService = Substitute.For<IHashService>();
        _sessionRepository = Substitute.For<ISessionRepository>();
        _accessTokenService = Substitute.For<IAccessTokenService>();
        _refreshTokenService = Substitute.For<IRefreshTokenService>();

        _handler = new RefreshAccessToken.Handler(
            _hashService,
            _sessionRepository,
            _accessTokenService,
            _refreshTokenService);
    }

    [Fact]
    public async Task Handle_CurrentRefreshToken_StoresNewTokensAsPendingAndReturnsRawTokens()
    {
        var command = new RefreshAccessToken.Command("mock-refreshToken!1");
        var refreshTokenHash = "hash-refreshToken@2"u8.ToArray();
        var newAccessTokenHash = "hash-newAccessToken#3"u8.ToArray();
        var newRefreshTokenHash = "hash-newRefreshToken$4"u8.ToArray();
        var session = new Session(
            SessionId.Random(),
            UserId.Random(),
            "hash-currentAccessToken%5"u8.ToArray(),
            refreshTokenHash);
        _hashService.HashAsync("mock-refreshToken!1", Arg.Any<CancellationToken>()).Returns(refreshTokenHash);
        _sessionRepository.GetByRefreshTokenAsync(refreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _accessTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newAccessToken^6");
        _refreshTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newRefreshToken&7");
        _hashService.HashAsync("mock-newAccessToken^6", Arg.Any<CancellationToken>()).Returns(newAccessTokenHash);
        _hashService.HashAsync("mock-newRefreshToken&7", Arg.Any<CancellationToken>()).Returns(newRefreshTokenHash);
        _sessionRepository.RotateRefreshTokenAsync(
                refreshTokenHash,
                newAccessTokenHash,
                newRefreshTokenHash,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("mock-newAccessToken^6");
        result.RefreshToken.Should().Be("mock-newRefreshToken&7");
        session.CurrentRefreshTokenHash.Should().BeSameAs(refreshTokenHash);
        session.PendingAccessTokenHash.Should().BeNull();
        session.PendingRefreshTokenHash.Should().BeNull();
        await _sessionRepository.Received().RotateRefreshTokenAsync(
            refreshTokenHash,
            newAccessTokenHash,
            newRefreshTokenHash,
            Arg.Any<CancellationToken>());
        await _sessionRepository.DidNotReceive().UpsertAsync(Arg.Any<Session>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PendingRefreshToken_PromotesPendingTokensThenStoresNewPendingTokens()
    {
        var command = new RefreshAccessToken.Command("mock-pendingRefreshToken!1");
        var pendingAccessTokenHash = "hash-pendingAccessToken@2"u8.ToArray();
        var pendingRefreshTokenHash = "hash-pendingRefreshToken#3"u8.ToArray();
        var newAccessTokenHash = "hash-newAccessToken$4"u8.ToArray();
        var newRefreshTokenHash = "hash-newRefreshToken%5"u8.ToArray();
        var session = new Session(
            SessionId.Random(),
            UserId.Random(),
            "hash-currentAccessToken^6"u8.ToArray(),
            "hash-currentRefreshToken&7"u8.ToArray(),
            pendingAccessTokenHash,
            pendingRefreshTokenHash);
        _hashService.HashAsync("mock-pendingRefreshToken!1", Arg.Any<CancellationToken>()).Returns(pendingRefreshTokenHash);
        _sessionRepository.GetByRefreshTokenAsync(pendingRefreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _accessTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newAccessToken*8");
        _refreshTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newRefreshToken(9");
        _hashService.HashAsync("mock-newAccessToken*8", Arg.Any<CancellationToken>()).Returns(newAccessTokenHash);
        _hashService.HashAsync("mock-newRefreshToken(9", Arg.Any<CancellationToken>()).Returns(newRefreshTokenHash);
        _sessionRepository.RotateRefreshTokenAsync(
                pendingRefreshTokenHash,
                newAccessTokenHash,
                newRefreshTokenHash,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("mock-newAccessToken*8");
        result.RefreshToken.Should().Be("mock-newRefreshToken(9");
        session.CurrentAccessTokenHash.Should().NotBeSameAs(pendingAccessTokenHash);
        session.CurrentRefreshTokenHash.Should().NotBeSameAs(pendingRefreshTokenHash);
        await _sessionRepository.Received().RotateRefreshTokenAsync(
            pendingRefreshTokenHash,
            newAccessTokenHash,
            newRefreshTokenHash,
            Arg.Any<CancellationToken>());
        await _sessionRepository.DidNotReceive().UpsertAsync(Arg.Any<Session>(), Arg.Any<CancellationToken>());
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
        await _sessionRepository.DidNotReceive()
            .RotateRefreshTokenAsync(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
        await _sessionRepository.DidNotReceive().UpsertAsync(Arg.Any<Session>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var command = new RefreshAccessToken.Command("mock-refreshToken!1");
        var refreshTokenHash = "hash-refreshToken@2"u8.ToArray();
        var session = new Session(
            SessionId.Random(),
            UserId.Random(),
            "hash-currentAccessToken#3"u8.ToArray(),
            refreshTokenHash);
        _hashService.HashAsync("mock-refreshToken!1", Arg.Any<CancellationToken>()).Returns(refreshTokenHash);
        _sessionRepository.GetByRefreshTokenAsync(refreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _accessTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newAccessToken$4");
        _refreshTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-newRefreshToken%5");
        var newAccessTokenHash = "hash-newAccessToken^6"u8.ToArray();
        var newRefreshTokenHash = "hash-newRefreshToken&7"u8.ToArray();
        _hashService.HashAsync("mock-newAccessToken$4", Arg.Any<CancellationToken>()).Returns(newAccessTokenHash);
        _hashService.HashAsync("mock-newRefreshToken%5", Arg.Any<CancellationToken>()).Returns(newRefreshTokenHash);
        _sessionRepository.RotateRefreshTokenAsync(
                refreshTokenHash,
                newAccessTokenHash,
                newRefreshTokenHash,
                Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, cancellationSource.Token);

        await _hashService.Received().HashAsync("mock-refreshToken!1", cancellationSource.Token);
        await _sessionRepository.Received().GetByRefreshTokenAsync(refreshTokenHash, cancellationSource.Token);
        await _accessTokenService.Received().CreateAsync(cancellationSource.Token);
        await _refreshTokenService.Received().CreateAsync(cancellationSource.Token);
        await _hashService.Received().HashAsync("mock-newAccessToken$4", cancellationSource.Token);
        await _hashService.Received().HashAsync("mock-newRefreshToken%5", cancellationSource.Token);
        await _sessionRepository.Received().RotateRefreshTokenAsync(
            refreshTokenHash,
            newAccessTokenHash,
            newRefreshTokenHash,
            cancellationSource.Token);
    }
}