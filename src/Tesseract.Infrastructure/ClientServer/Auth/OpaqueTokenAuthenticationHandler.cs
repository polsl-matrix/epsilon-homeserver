using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Models;
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

        if (response.User is not { } user || response.SessionId is not { } sessionId)
        {
            return AuthenticateResult.Fail("User session was not found or has expired.");
        }

        var claims = MapSessionToClaims(user, sessionId);

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;

        var hasAuthorizationHeader = Request.Headers.ContainsKey(HeaderNames.Authorization);
        var response = hasAuthorizationHeader
            ? new MatrixAuthErrorResponse("M_UNKNOWN_TOKEN", "Unrecognised access token.")
            : new MatrixAuthErrorResponse("M_MISSING_TOKEN", "Missing access token.");

        await WriteMatrixErrorAsync(response);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;

        await WriteMatrixErrorAsync(new MatrixAuthErrorResponse("M_FORBIDDEN"));
    }

    private async Task WriteMatrixErrorAsync(MatrixAuthErrorResponse response)
    {
        Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(response);

        await Response.WriteAsync(body);
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

    private static IReadOnlyList<Claim> MapSessionToClaims(User user, SessionId session)
    {
        var userId = user.Id.Value.ToString();
        var sessionId = session.Value.ToString();

        return
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Sid, sessionId),
        ];
    }

    private sealed record MatrixAuthErrorResponse(
        [property: JsonPropertyName("errcode")]
        string Code,
        [property: JsonPropertyName("error")]
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? Message = null);
}