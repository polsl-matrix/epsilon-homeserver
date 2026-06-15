using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Web.ClientServer.Auth.Contracts;

namespace Tesseract.Web.ClientServer.Auth;

[ApiController]
[Route("_matrix/client")]
// TODO: Add rate limiter.
public class AuthController(IMediator mediator) : ControllerBase
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

    [HttpGet("v3/account/whoami")]
    [Authorize]
    public ActionResult<WhoAmIResponse> WhoAmI()
    {
        var userId = User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        return Ok(new WhoAmIResponse
        {
            UserId = userId,
        });
    }
}