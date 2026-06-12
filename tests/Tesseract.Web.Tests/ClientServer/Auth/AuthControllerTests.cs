using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Auth;
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

    public static TheoryData<Handle> ValidHandles =>
    [
        new("jerry", "example.com"),
        new("mike", "math.lovers"),
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
    [MemberData(nameof(ValidHandles))]
    public async Task AuthenticateUser_MediatorReturnsResponse_MapsValuesCorrectly(Handle handle)
    {
        var request = CreateEmptyAuthenticateUserRequest();
        var response = new LoginUser.Response(handle, string.Empty, string.Empty);
        _mediator.Send(Arg.Any<LoginUser.Command>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _controller.AuthenticateUser(
            request, CancellationToken.None);

        result.Handle.Should().Be(handle.ToString());
    }

    [Fact]
    public async Task AuthenticateUser_CancellationTokenProvided_PassesSameTokenToMediator()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var request = CreateEmptyAuthenticateUserRequest();
        var response = CreateEmptyLoginUserResponse();
        _mediator.Send(Arg.Any<LoginUser.Command>(), cancellationToken)
            .Returns(response);

        await _controller.AuthenticateUser(request, cancellationToken);

        await _mediator.Received().Send(Arg.Any<LoginUser.Command>(), cancellationToken);
    }

    [Fact]
    public async Task AuthenticateUser_MediatorThrowsException_PropagatesException()
    {
        var exception = new InvalidOperationException("Something went wrong.");

        var request = CreateEmptyAuthenticateUserRequest();
        _mediator.Send(Arg.Any<LoginUser.Command>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var act = async () => await _controller.AuthenticateUser(request, CancellationToken.None);

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

    private static LoginUser.Response CreateEmptyLoginUserResponse() => new(
        new Handle("localpart", "domain"),
        string.Empty, string.Empty
    );
}