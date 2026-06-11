using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Domain.Users.Entities;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class LoginUserTests
{
    private readonly IAuthenticationFlow _flow;
    private readonly LoginUser.Handler _handler;

    public LoginUserTests()
    {
        _flow = Substitute.For<IAuthenticationFlow>();
        _flow.Type.Returns("t.test.flow");

        _handler = new LoginUser.Handler([_flow]);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthenticatedUser()
    {
        var command = new LoginUser.Command("mike", "password123", _flow.Type);
        var user = new User(Guid.NewGuid(), new Handle("mike123", "loves.maths"));

        _flow.AuthenticateAsync(command.User, command.Password, Arg.Any<CancellationToken>())
            .Returns(user);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.User.Should().BeSameAs(user);
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
        var command = new LoginUser.Command("colt", "my_happy_password", _flow.Type);
        var user = new User(Guid.NewGuid(), new Handle("colt", "happi-happi.happi"));

        var cancellationSource = new CancellationTokenSource();

        _flow.AuthenticateAsync(command.User, command.Password, cancellationSource.Token)
            .Returns(user);

        await _handler.Handle(command, cancellationSource.Token);

        await _flow.Received().AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationSource.Token);
    }
}