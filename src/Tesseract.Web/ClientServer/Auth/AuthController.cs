using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Web.ClientServer.Auth.Contracts;

namespace Tesseract.Web.ClientServer.Auth;

[ApiController]
[Route("_matrix/client")]
// TODO: Add rate limiter.
public class AuthController(IMediator mediator)
{
    [HttpGet("v3/login")]
    [AllowAnonymous]
    public async Task<GetSupportedAuthenticationFlowsResponse> GetSupportedAuthenticationFlows(
        CancellationToken cancellationToken)
    {
        var query = new GetSupportedAuthenticationFlows.Query();

        var result = await mediator.Send(query, cancellationToken);

        var flows = result.Flows
            .Select(flow => new LoginFlow
            {
                Type = flow.Type,
            })
            .ToList();

        return new GetSupportedAuthenticationFlowsResponse
        {
            Flows = flows,
        };
    }

    [HttpPost("v3/login")]
    [AllowAnonymous]
    public async Task<AuthenticateUserResponse> AuthenticateUser(
        AuthenticateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginUser.Command(
            request.Identifier.User,
            request.Password,
            request.Type
        );

        var result = await mediator.Send(command, cancellationToken);

        return new AuthenticateUserResponse
        {
            Handle = result.Handle.ToString(),
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
        };
    }

    [HttpPost("v3/register")]
    [AllowAnonymous]
    public async Task<RegisterAccountResponse> RegisterAccount(
        RegisterAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterAccount.Command(
            request.Username,
            request.Password);

        var result = await mediator.Send(command, cancellationToken);

        return new RegisterAccountResponse
        {
            UserId = result.Handle.ToString(),
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
        };
    }

    [HttpPost("v3/logout")]
    [Authorize]
    public async Task<LogoutUserResponse> LogoutUser(
        [FromHeader(Name = "Authorization")] string authorization, CancellationToken cancellationToken)
    {
        if (!AuthenticationHeaderValue.TryParse(authorization, out var header) ||
            !string.Equals(header.Scheme, "Bearer", StringComparison.InvariantCultureIgnoreCase) ||
            header.Parameter is not { } accessToken)
        {
            throw new InvalidOperationException("Authenticated request does not contain a bearer token.");
        }

        var command = new LogoutUser.Command(accessToken);

        await mediator.Send(command, cancellationToken);

        return new LogoutUserResponse();
    }
}