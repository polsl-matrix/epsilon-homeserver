using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class DeactivateAccountTests
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;

    private readonly DeactivateAccount.Handler _handler;

    public DeactivateAccountTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _sessionRepository = Substitute.For<ISessionRepository>();

        _handler = new DeactivateAccount.Handler(_userRepository, _sessionRepository);
    }

    [Fact]
    public async Task Handle_ValidUser_MarksUserDeactivatedAndDeletesSessions()
    {
        var userId = UserId.Random();
        var command = new DeactivateAccount.Command(userId);

        await _handler.Handle(command, CancellationToken.None);

        await _userRepository.Received().MarkDeactivatedAsync(userId, Arg.Any<CancellationToken>());
        await _sessionRepository.Received().DeleteAllByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsNoSupportUnbindResult()
    {
        var command = new DeactivateAccount.Command(UserId.Random());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IdServerUnbindResult.Should().Be("no-support");
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var userId = UserId.Random();
        var command = new DeactivateAccount.Command(userId);

        await _handler.Handle(command, cancellationToken);

        await _userRepository.Received().MarkDeactivatedAsync(userId, cancellationToken);
        await _sessionRepository.Received().DeleteAllByUserIdAsync(userId, cancellationToken);
    }
}