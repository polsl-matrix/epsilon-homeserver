using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Reflection;
using System.Security.Claims;
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
    public async Task RegisterAccount_RequestContainsData_PassesSameDataToMediator()
    {
        var request = new RegisterAccountRequest
        {
            Username = "puffin",
            Password = "penguin1",
        };

        RegisterAccount.Command? calledCommand = null;
        _mediator.Send(Arg.Any<RegisterAccount.Command>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterAccount.Response(new UserHandle("puffin", "north.pole"), string.Empty, string.Empty))
            .AndDoes(call => calledCommand = call.Arg<RegisterAccount.Command>());

        await _controller.RegisterAccount(request, CancellationToken.None);

        calledCommand.Should().NotBeNull();
        calledCommand.Username.Should().Be("puffin");
        calledCommand.Password.Should().Be("penguin1");
    }

    [Fact]
    public async Task RegisterAccount_MediatorReturnsResponse_MapsValuesCorrectly()
    {
        var request = new RegisterAccountRequest
        {
            Username = "le_fish",
            Password = "monsieur",
        };
        _mediator.Send(Arg.Any<RegisterAccount.Command>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterAccount.Response(new UserHandle("le_fish", "baguette.muah"), "mock-accessToken!1", "mock-refreshToken@2"));

        var result = await _controller.RegisterAccount(request, CancellationToken.None);

        result.UserId.Should().Be("@le_fish:baguette.muah");
        result.AccessToken.Should().Be("mock-accessToken!1");
        result.RefreshToken.Should().Be("mock-refreshToken@2");
    }

    [Fact]
    public async Task RegisterAccount_CancellationTokenProvided_PassesSameTokenToMediator()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var request = new RegisterAccountRequest
        {
            Username = "wastebin",
            Password = "tr@sh",
        };
        _mediator.Send(Arg.Any<RegisterAccount.Command>(), cancellationToken)
            .Returns(new RegisterAccount.Response(new UserHandle("wastebin", "messy.streets"), string.Empty, string.Empty));

        await _controller.RegisterAccount(request, cancellationToken);

        await _mediator.Received().Send(Arg.Any<RegisterAccount.Command>(), cancellationToken);
    }

    [Fact]
    public void WhoAmI_AuthenticatedUser_ReturnsUserId()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, "@jerry:example.com")],
                    "test")),
            },
        };

        var result = _controller.WhoAmI();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WhoAmIResponse>().Subject;
        response.UserId.Should().Be("@jerry:example.com");
    }

    [Fact]
    public void WhoAmI_RequiresAuthorization()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.WhoAmI), BindingFlags.Instance | BindingFlags.Public);

        method.Should().NotBeNull();
        method!.GetCustomAttribute<AuthorizeAttribute>().Should().NotBeNull();
        method!.GetCustomAttribute<AllowAnonymousAttribute>().Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhoAmI_MissingNameClaim_ReturnsUnauthorized(string? claimValue)
    {
        var claims = claimValue is null
            ? Array.Empty<Claim>()
            : [new Claim(ClaimTypes.Name, claimValue)];

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")),
            },
        };

        var result = _controller.WhoAmI();

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
}