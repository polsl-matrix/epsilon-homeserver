using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class AuthenticateUserTests
{
    private readonly IHashService _hashService;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;

    private readonly AuthenticateUser.Handler _handler;

    public AuthenticateUserTests()
    {
        _hashService = Substitute.For<IHashService>();
        _sessionRepository = Substitute.For<ISessionRepository>();
        _userRepository = Substitute.For<IUserRepository>();

        _handler = new AuthenticateUser.Handler(_hashService, _sessionRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_ValidAccessToken_ReturnsUser()
    {
        var command = new AuthenticateUser.Command("mock-accessToken!1");

        var refreshTokenHash = "hash-accessToken@2"u8.ToArray();

        var user = new User(UserId.Random(), new UserHandle("harry", "mel.on"));
        var session = new Session(SessionId.Random(), user.Id, refreshTokenHash, "hash-refreshToken#3"u8.ToArray());
        _hashService.HashAsync("mock-accessToken!1", Arg.Any<CancellationToken>()).Returns(refreshTokenHash);
        _sessionRepository.GetByAccessTokenAsync(refreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.User.Should().NotBeNull();
        result.User.Should().BeSameAs(user);
    }

    [Fact]
    public async Task Handle_IncorrectAccessToken_ReturnsNull()
    {
        var command = new AuthenticateUser.Command("mock-incorrectToken!1");

        _sessionRepository.GetByAccessTokenAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns((Session?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.User.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeactivatedUser_ReturnsNull()
    {
        var command = new AuthenticateUser.Command("mock-accessToken!1");

        var accessTokenHash = "hash-accessToken@2"u8.ToArray();
        var user = new User(UserId.Random(), new UserHandle("harry", "mel.on"), true);
        var session = new Session(SessionId.Random(), user.Id, accessTokenHash, "hash-refreshToken#3"u8.ToArray());
        _hashService.HashAsync("mock-accessToken!1", Arg.Any<CancellationToken>()).Returns(accessTokenHash);
        _sessionRepository.GetByAccessTokenAsync(accessTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.User.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();

        var command = new AuthenticateUser.Command("mock-accessToken!1");
        var refreshTokenHash = "hash-accessToken@2"u8.ToArray();

        var user = new User(UserId.Random(), new UserHandle("jake", "smith.ukulele"));
        var session = new Session(SessionId.Random(), user.Id, refreshTokenHash, "hash-refreshToken#3"u8.ToArray());
        _hashService.HashAsync("mock-accessToken!1", Arg.Any<CancellationToken>()).Returns(refreshTokenHash);
        _sessionRepository.GetByAccessTokenAsync(refreshTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(command, cancellationSource.Token);

        await _hashService.Received().HashAsync(Arg.Any<string>(), cancellationSource.Token);
        await _sessionRepository.Received().GetByAccessTokenAsync(Arg.Any<byte[]>(), cancellationSource.Token);
        await _userRepository.Received().GetByIdAsync(Arg.Any<UserId>(), cancellationSource.Token);
    }
}