using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Tesseract.Application.ClientServer.Auth.UseCases;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;
using Tesseract.Infrastructure.ClientServer.Auth;

namespace Tesseract.Infrastructure.Tests.ClientServer.Auth;

public class OpaqueTokenAuthenticationHandlerTests
{
    private readonly IMediator _mediator;
    private readonly HttpContext _httpContext;

    private readonly OpaqueTokenAuthenticationHandler _handler;

    public OpaqueTokenAuthenticationHandlerTests()
    {
        _mediator = Substitute.For<IMediator>();
        var options = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        var logger = Substitute.For<ILoggerFactory>();
        var encoder = Substitute.For<UrlEncoder>();
        var clock = Substitute.For<ISystemClock>();

        _httpContext = new DefaultHttpContext();

        _handler = new OpaqueTokenAuthenticationHandler(
            _mediator, options, logger, encoder, clock);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_MissingAuthorizationHeader_ReturnsNoResult()
    {
        var scheme = new AuthenticationScheme("Bearer", "Bearer", typeof(OpaqueTokenAuthenticationHandler));
        await _handler.InitializeAsync(scheme, _httpContext);

        var result = await _handler.AuthenticateAsync();

        result.None.Should().BeTrue();
        await _mediator.DidNotReceive().Send(Arg.Any<AuthenticateUser.Command>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAuthenticateAsync_InvalidAuthorizationHeaderFormat_ReturnsNoResult()
    {
        _httpContext.Request.Headers[HeaderNames.Authorization] = "whoops*oo*o*ops";

        var scheme = new AuthenticationScheme("Bearer", "Bearer", typeof(OpaqueTokenAuthenticationHandler));
        await _handler.InitializeAsync(scheme, _httpContext);

        var result = await _handler.AuthenticateAsync();

        result.None.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_IncorrectScheme_ReturnsNoResult()
    {
        _httpContext.Request.Headers[HeaderNames.Authorization] = "Basic thisIsNot!TheRightScheme@";

        var scheme = new AuthenticationScheme("Bearer", "Bearer", typeof(OpaqueTokenAuthenticationHandler));
        await _handler.InitializeAsync(scheme, _httpContext);

        var result = await _handler.AuthenticateAsync();

        result.None.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ValidToken_SendsCommandToMediator()
    {
        const string token = "mock-accessToken!1";

        var scheme = new AuthenticationScheme("Bearer", "Bearer", typeof(OpaqueTokenAuthenticationHandler));
        await _handler.InitializeAsync(scheme, _httpContext);
        _httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {token}";

        _mediator.Send(Arg.Is<AuthenticateUser.Command>(c => c.AccessToken == token), Arg.Any<CancellationToken>())
            .Returns(new AuthenticateUser.Response(null));

        await _handler.AuthenticateAsync();

        await _mediator.Received().Send(Arg.Is<AuthenticateUser.Command>(c => c.AccessToken == token), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAuthenticateAsync_MediatorReturnsNullUser_ReturnsFailure()
    {
        var scheme = new AuthenticationScheme("Bearer", "Bearer", typeof(OpaqueTokenAuthenticationHandler));
        await _handler.InitializeAsync(scheme, _httpContext);
        _httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer invalid-accessToken@2";

        _mediator
            .Send(Arg.Any<AuthenticateUser.Command>(), Arg.Any<CancellationToken>())
            .Returns(new AuthenticateUser.Response(null));

        var result = await _handler.AuthenticateAsync();

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure.Message.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ValidCredential_ReturnsSuccessTicket()
    {
        // Arrange
        _httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer valid-token";

        var scheme = new AuthenticationScheme("scheme-name#1", "Scheme Name #1", typeof(OpaqueTokenAuthenticationHandler));
        await _handler.InitializeAsync(scheme, _httpContext);

        var user = new User(UserId.Random(), new Handle("jack", "black.cherry"));
        _mediator.Send(Arg.Any<AuthenticateUser.Command>(), Arg.Any<CancellationToken>())
            .Returns(new AuthenticateUser.Response(user));

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Ticket.Should().NotBeNull();
        result.Ticket.AuthenticationScheme.Should().Be("scheme-name#1");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_SuccessfulAuth_MapsCorrectClaims()
    {
        // Arrange
        _httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer valid-token";

        var scheme = new AuthenticationScheme("Bearer", "Bearer", typeof(OpaqueTokenAuthenticationHandler));
        await _handler.InitializeAsync(scheme, _httpContext);

        var user = new User(UserId.Random(), new Handle("6fire7", "water.flows"));
        _mediator
            .Send(Arg.Any<AuthenticateUser.Command>(), Arg.Any<CancellationToken>())
            .Returns(new AuthenticateUser.Response(user));

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        var principal = result.Ticket.Principal;
        var nameIdentifierClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        nameIdentifierClaim.Should().NotBeNull();
        nameIdentifierClaim!.Value.Should().Be(user.Id.Value.ToString());
    }
}