using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;
using Tesseract.Web.ClientServer.Auth;
using Tesseract.Web.ClientServer.Auth.Contracts;

namespace Tesseract.Web.Tests.ClientServer.Auth;

public class AuthControllerTests
{
    private readonly IMediator _mediator;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _controller = new AuthController(_mediator);
    }

    public static TheoryData<User> ValidUsers =>
    [
        new(Guid.Parse("75bad405-91c3-46bc-b78f-930e35925435"), new Handle("jerry", "example.com")),
        new(Guid.Parse("831fc713-ab65-4bf5-85de-320968b26eeb"), new Handle("mike", "math.lovers")),
    ];

    [Theory]
    [InlineData("jerry@example.com", "password123", "t.login.any")]
    [InlineData("mike@math.lovers", "987secrets", "u.any.type")]
    [InlineData("sid@ice.skater", null, "v.empty.fields")]
    [InlineData(null, "only-password", "v.empty.fields")]
    [InlineData(null, null, "v.empty.fields")]
    public async Task AuthenticateUser_RequestContainsData_PassesSameDataToMediator(
        string? handle, string? password, string type)
    {
        // Arrange
        var request = new AuthenticateUserRequest
        {
            Identifier = new AuthenticateUserRequest.UserIdentifier
            {
                User = handle,
            },
            Password = password,
            Type = type,
        };
        var response = CreateEmptyLoginUserResponse();

        LoginUser.Command? calledCommand = null;

        _mediator.Send(Arg.Any<LoginUser.Command>(), Arg.Any<CancellationToken>())
            .Returns(response).AndDoes(call => calledCommand = call.Arg<LoginUser.Command>());

        // Act
        await _controller.AuthenticateUser(request, CancellationToken.None);

        // Assert
        await _mediator.Received().Send(Arg.Any<LoginUser.Command>(), Arg.Any<CancellationToken>());

        calledCommand.Should().NotBeNull();
        calledCommand.User.Should().Be(handle);
        calledCommand.Password.Should().Be(password);
        calledCommand.Type.Should().Be(type);
    }

    [Theory]
    [MemberData(nameof(ValidUsers))]
    public async Task AuthenticateUser_MediatorReturnsResponse_MapsValuesCorrectly(User user)
    {
        // Arrange
        var request = CreateEmptyAuthenticateUserRequest();
        var response = new LoginUser.Response(user, string.Empty, string.Empty);

        _mediator.Send(Arg.Any<LoginUser.Command>(), Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _controller.AuthenticateUser(
            request, CancellationToken.None);

        // Assert
        result.Handle.Should().Be(user.Handle.ToString());
    }

    [Fact]
    public async Task AuthenticateUser_CancellationTokenProvided_PassesSameTokenToMediator()
    {
        // Arrange
        var request = CreateEmptyAuthenticateUserRequest();
        var response = CreateEmptyLoginUserResponse();

        var cancellationToken = new CancellationTokenSource().Token;

        _mediator.Send(Arg.Any<LoginUser.Command>(), cancellationToken)
            .Returns(response);

        // Act
        await _controller.AuthenticateUser(request, cancellationToken);

        // Assert
        await _mediator.Received().Send(Arg.Any<LoginUser.Command>(), cancellationToken);
    }

    [Fact]
    public async Task AuthenticateUser_MediatorThrowsException_PropagatesException()
    {
        // Arrange
        var request = CreateEmptyAuthenticateUserRequest();

        var exception = new InvalidOperationException("Something went wrong.");

        _mediator.Send(Arg.Any<LoginUser.Command>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        var act = async () => await _controller.AuthenticateUser(request, CancellationToken.None);

        // Assert
        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(exception);
    }

    private static AuthenticateUserRequest CreateEmptyAuthenticateUserRequest() => new()
    {
        Identifier = new AuthenticateUserRequest.UserIdentifier
        {
            User = null,
        },
        Type = string.Empty,
    };

    private static LoginUser.Response CreateEmptyLoginUserResponse()
    {
        var user = new User(Guid.Empty, new Handle("localpart", "domain"));
        return new LoginUser.Response(user, string.Empty, string.Empty);
    }
}