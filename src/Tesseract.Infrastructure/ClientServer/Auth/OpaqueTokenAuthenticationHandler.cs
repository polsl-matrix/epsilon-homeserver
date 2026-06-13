using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Domain.Users;

namespace Tesseract.Infrastructure.ClientServer.Auth;

public class OpaqueTokenAuthenticationHandler(
    IMediator mediator,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISystemClock clock)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (GetToken() is not { } token)
        {
            return AuthenticateResult.NoResult();
        }

        var response = await mediator.Send(new AuthenticateUser.Command(token));

        if (response.User is not { } user)
        {
            return AuthenticateResult.Fail("User session was not found or has expired.");
        }

        var claims = MapUserToClaims(user);

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private string? GetToken()
    {
        var authorizationHeaderName = Request.Headers[HeaderNames.Authorization];

        if (!AuthenticationHeaderValue.TryParse(authorizationHeaderName, out var header))
        {
            return null;
        }

        if (!string.Equals(header.Scheme, SchemeNames.Bearer, StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return header.Parameter;
    }

    private static IReadOnlyList<Claim> MapUserToClaims(User user)
    {
        var userId = user.Id.Value.ToString();

        return
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
        ];
    }
}