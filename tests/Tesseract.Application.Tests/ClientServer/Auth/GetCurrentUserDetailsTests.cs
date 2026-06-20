using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class GetCurrentUserDetailsTests
{
    private readonly IHashService _hashService;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;

    private readonly GetCurrentUserDetails.Handler _handler;

    public GetCurrentUserDetailsTests()
    {
        _hashService = Substitute.For<IHashService>();
        _sessionRepository = Substitute.For<ISessionRepository>();
        _userRepository = Substitute.For<IUserRepository>();

        _handler = new GetCurrentUserDetails.Handler(_hashService, _sessionRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_ValidAccessToken_ReturnsUserIdFromPersistedUser()
    {
        var command = new GetCurrentUserDetails.Query("mock-accessToken!1");
        var accessTokenHash = "hash-accessToken@2"u8.ToArray();

        var user = new User(UserId.Random(), new UserHandle("maple", "tree.house"));
        var session = new Session(SessionId.Random(), user.Id, accessTokenHash, "hash-refreshToken#3"u8.ToArray());
        _hashService.HashAsync(command.AccessToken, Arg.Any<CancellationToken>()).Returns(accessTokenHash);
        _sessionRepository.GetByAccessTokenAsync(accessTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.UserId.Should().Be(user.Handle);
    }

    [Fact]
    public async Task Handle_UnknownAccessToken_ThrowsUnknownAccessTokenException()
    {
        var command = new GetCurrentUserDetails.Query("unknown-access-token");
        var accessTokenHash = "hash-accessToken@2"u8.ToArray();
        _hashService.HashAsync(command.AccessToken, Arg.Any<CancellationToken>()).Returns(accessTokenHash);
        _sessionRepository.GetByAccessTokenAsync(accessTokenHash, Arg.Any<CancellationToken>()).Returns((Session?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnknownAccessTokenException>();
    }

    [Fact]
    public async Task Handle_SessionReferencesMissingUser_ThrowsUnknownAccessTokenException()
    {
        var command = new GetCurrentUserDetails.Query("mock-accessToken!1");
        var accessTokenHash = "hash-accessToken@2"u8.ToArray();
        var session = new Session(SessionId.Random(), UserId.Random(), accessTokenHash, "hash-refreshToken#3"u8.ToArray());
        _hashService.HashAsync(command.AccessToken, Arg.Any<CancellationToken>()).Returns(accessTokenHash);
        _sessionRepository.GetByAccessTokenAsync(accessTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _userRepository.GetByIdAsync(session.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnknownAccessTokenException>();
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var command = new GetCurrentUserDetails.Query("mock-accessToken!1");
        var accessTokenHash = "hash-accessToken@2"u8.ToArray();

        var user = new User(UserId.Random(), new UserHandle("crow", "nest.place"));
        var session = new Session(SessionId.Random(), user.Id, accessTokenHash, "hash-refreshToken#3"u8.ToArray());
        _hashService.HashAsync(command.AccessToken, Arg.Any<CancellationToken>()).Returns(accessTokenHash);
        _sessionRepository.GetByAccessTokenAsync(accessTokenHash, Arg.Any<CancellationToken>()).Returns(session);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(command, cancellationSource.Token);

        await _hashService.Received().HashAsync(command.AccessToken, cancellationSource.Token);
        await _sessionRepository.Received().GetByAccessTokenAsync(accessTokenHash, cancellationSource.Token);
        await _userRepository.Received().GetByIdAsync(user.Id, cancellationSource.Token);
    }
}