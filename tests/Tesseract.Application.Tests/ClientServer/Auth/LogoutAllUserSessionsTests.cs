using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class LogoutAllUserSessionsTests
{
    private readonly ISessionRepository _sessionRepository;
    private readonly LogoutAllUserSessions.Handler _handler;

    public LogoutAllUserSessionsTests()
    {
        _sessionRepository = Substitute.For<ISessionRepository>();
        _handler = new LogoutAllUserSessions.Handler(_sessionRepository);
    }

    [Fact]
    public async Task Handle_CommandProvided_DeletesAllSessionsForUser()
    {
        var userId = UserId.Random();
        var command = new LogoutAllUserSessions.Command(userId);

        await _handler.Handle(command, CancellationToken.None);

        await _sessionRepository.Received().DeleteAllByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var userId = UserId.Random();
        var command = new LogoutAllUserSessions.Command(userId);

        await _handler.Handle(command, cancellationToken);

        await _sessionRepository.Received().DeleteAllByUserIdAsync(Arg.Any<UserId>(), cancellationToken);
    }
}