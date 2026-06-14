using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Reflection;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Domain.Users.Values;
using Tesseract.Web.ClientServer.Auth;
using Tesseract.Web.ClientServer.Auth.Contracts;
using LoginFlow = Tesseract.Application.ClientServer.Auth.Models.LoginFlow;

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

    public static TheoryData<UserHandle> ValidUserHandles =>
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
    [MemberData(nameof(ValidUserHandles))]
    public async Task AuthenticateUser_MediatorReturnsResponse_MapsValuesCorrectly(UserHandle handle)
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
        new UserHandle("localpart", "domain"),
        string.Empty, string.Empty
    );

    [Fact]
    public async Task GetSupportedAuthenticationFlows_MediatorReturnsFlows_MapsValuesCorrectly()
    {
        var applicationFlows = new List<LoginFlow>
        {
            new("m.login.password"),
            new("m.login.sso"),
        };
        _mediator.Send(Arg.Any<GetSupportedAuthenticationFlows.Query>(), Arg.Any<CancellationToken>())
            .Returns(new GetSupportedAuthenticationFlows.Response(applicationFlows));

        var result = await _controller.GetSupportedAuthenticationFlows(CancellationToken.None);

        result.Flows.Should().HaveCount(2);
        result.Flows[0].Type.Should().Be("m.login.password");
        result.Flows[1].Type.Should().Be("m.login.sso");
    }

    [Fact]
    public async Task GetSupportedAuthenticationFlows_MediatorReturnsNoFlows_ReturnsEmptyFlowsList()
    {
        var mediatorResponse = new GetSupportedAuthenticationFlows.Response([]);
        _mediator.Send(Arg.Any<GetSupportedAuthenticationFlows.Query>(), Arg.Any<CancellationToken>())
            .Returns(mediatorResponse);

        var result = await _controller.GetSupportedAuthenticationFlows(CancellationToken.None);

        result.Flows.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSupportedAuthenticationFlows_CancellationTokenProvided_PassesSameTokenToMediator()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var mediatorResponse = new GetSupportedAuthenticationFlows.Response([]);
        _mediator.Send(Arg.Any<GetSupportedAuthenticationFlows.Query>(), cancellationToken)
            .Returns(mediatorResponse);

        await _controller.GetSupportedAuthenticationFlows(cancellationToken);

        await _mediator.Received().Send(Arg.Any<GetSupportedAuthenticationFlows.Query>(), cancellationToken);
    }

    [Fact]
    public void Controller_Always_EnablesAuthRateLimiting()
    {
        var attribute = typeof(AuthController).GetCustomAttribute<EnableRateLimitingAttribute>();

        attribute.Should().NotBeNull();
        attribute?.PolicyName.Should().Be("auth");
    }

    [Fact]
    public async Task RefreshAccessToken_RequestContainsRefreshToken_PassesSameDataToMediator()
    {
        var request = new RefreshAccessTokenRequest
        {
            RefreshToken = "mock-refreshToken!1",
        };
        RefreshAccessToken.Command? calledCommand = null;
        _mediator.Send(Arg.Any<RefreshAccessToken.Command>(), Arg.Any<CancellationToken>())
            .Returns(new RefreshAccessToken.Response(string.Empty, string.Empty))
            .AndDoes(call => calledCommand = call.Arg<RefreshAccessToken.Command>());

        await _controller.RefreshAccessToken(request, CancellationToken.None);

        await _mediator.Received().Send(Arg.Any<RefreshAccessToken.Command>(), Arg.Any<CancellationToken>());
        calledCommand.Should().NotBeNull();
        calledCommand.RefreshToken.Should().Be("mock-refreshToken!1");
    }

    [Fact]
    public async Task RefreshAccessToken_MediatorReturnsResponse_MapsValuesCorrectly()
    {
        var request = new RefreshAccessTokenRequest
        {
            RefreshToken = "mock-refreshToken!1",
        };
        _mediator.Send(Arg.Any<RefreshAccessToken.Command>(), Arg.Any<CancellationToken>())
            .Returns(new RefreshAccessToken.Response("mock-accessToken@2", "mock-refreshToken#3"));

        var result = await _controller.RefreshAccessToken(request, CancellationToken.None);

        result.AccessToken.Should().Be("mock-accessToken@2");
        result.RefreshToken.Should().Be("mock-refreshToken#3");
    }

    [Fact]
    public async Task RefreshAccessToken_CancellationTokenProvided_PassesSameTokenToMediator()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var request = new RefreshAccessTokenRequest
        {
            RefreshToken = string.Empty,
        };
        _mediator.Send(Arg.Any<RefreshAccessToken.Command>(), cancellationToken)
            .Returns(new RefreshAccessToken.Response(string.Empty, string.Empty));

        await _controller.RefreshAccessToken(request, cancellationToken);

        await _mediator.Received().Send(Arg.Any<RefreshAccessToken.Command>(), cancellationToken);
    }

    [Fact]
    public async Task RefreshAccessToken_MediatorThrowsException_PropagatesException()
    {
        var exception = new InvalidOperationException("Something went wrong.");
        var request = new RefreshAccessTokenRequest
        {
            RefreshToken = string.Empty,
        };
        _mediator.Send(Arg.Any<RefreshAccessToken.Command>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var act = async () => await _controller.RefreshAccessToken(request, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(exception);
    }

    [Fact]
    public void RefreshAccessToken_ActionAllowsAnonymousAccess()
    {
        var method = typeof(AuthController).GetMethod(
            nameof(AuthController.RefreshAccessToken),
            BindingFlags.Instance | BindingFlags.Public);

        method.Should().NotBeNull();
        method?.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
        method?.GetCustomAttribute<AuthorizeAttribute>().Should().BeNull();
    }
}