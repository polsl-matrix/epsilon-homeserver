using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Application.ClientServer.Auth.UseCases;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class LoginUserTests
{
    private readonly IAuthenticationFlow _flow;
    private readonly ISessionFactory _sessionFactory;
    private readonly ISessionRepository _sessionRepository;

    private readonly LoginUser.Handler _handler;

    public LoginUserTests()
    {
        _flow = Substitute.For<IAuthenticationFlow>();
        _flow.Type.Returns("t.test.flow");
        _sessionFactory = Substitute.For<ISessionFactory>();
        _sessionRepository = Substitute.For<ISessionRepository>();

        _handler = new LoginUser.Handler([_flow], _sessionFactory, _sessionRepository);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsUserAndTokens()
    {
        var command = new LoginUser.Command("tom", "paws%", _flow.Type);

        var user = new User(UserId.Random(), new Handle("tom", "whiskers.meow"));
        var session = new Session(SessionId.Random(), user.Id, "hash-accessToken#3"u8.ToArray(), "hash-refreshToken$4"u8.ToArray());
        _flow.AuthenticateAsync(command.User, command.Password, Arg.Any<CancellationToken>()).Returns(user);
        _sessionFactory.CreateAsync(user, Arg.Any<CancellationToken>()).Returns((session, "mock-accessToken!1", "mock-refreshToken@2"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Handle.Should().Be(user.Handle);
        result.AccessToken.Should().Be("mock-accessToken!1");
        result.RefreshToken.Should().Be("mock-refreshToken@2");
    }

    [Fact]
    public async Task Handle_UnsupportedAuthenticationFlow_ThrowsBadLoginTypeException()
    {
        var command = new LoginUser.Command("josh", "987maths", "t.unknown!");

        var act = () => _handler.Handle(command, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<BadLoginTypeException>();
        thrown.Which.Message.Should().Contain(command.Type);
        await _flow.DidNotReceive().AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IncorrectLoginDetails_ThrowsForbiddenException()
    {
        var command = new LoginUser.Command("mike", "wrong#password", _flow.Type);

        _flow.AuthenticateAsync(command.User, command.Password, CancellationToken.None)
            .Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();

        var command = new LoginUser.Command("colt", "my_happy_password", _flow.Type);
        var user = new User(UserId.Random(), new Handle("colt", "happi-happi.happi"));
        _flow.AuthenticateAsync(command.User, command.Password, cancellationSource.Token)
            .Returns(user);

        await _handler.Handle(command, cancellationSource.Token);

        await _flow.Received().AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationSource.Token);
        await _sessionFactory.Received().CreateAsync(Arg.Any<User>(), cancellationSource.Token);
        await _sessionRepository.Received().UpsertAsync(Arg.Any<Session>(), cancellationSource.Token);
    }
}