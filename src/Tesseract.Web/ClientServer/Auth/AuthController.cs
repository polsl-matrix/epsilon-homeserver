using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Web.ClientServer.Auth.Contracts;
using Tesseract.Web.ClientServer.Profile.Contracts;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.ClientServer.Auth;

[ApiController]
[Route("_matrix/client/v3")]
// TODO: Add rate limiter.
public class AuthController(IMediator mediator, ICurrentUser user)
{
    [HttpPost("register")]
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

    [HttpGet("register/available")]
    [AllowAnonymous]
    public async Task<RegisterAvailableResponse> CheckUsernameAvailability(
        [FromQuery] string username, CancellationToken cancellationToken)
    {
        var query = new CheckUsernameAvailability.Query(username);

        var result = await mediator.Send(query, cancellationToken);

        return new RegisterAvailableResponse
        {
            Available = result.Available,
        };
    }

    [HttpGet("login")]
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

    [HttpPost("login")]
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

    [HttpGet("account/whoami")]
    [Authorize]
    public async Task<GetCurrentUserDetailsResponse> GetCurrentUserDetails(CancellationToken cancellationToken)
    {
        var query = new GetCurrentSessionDetails.Query(user.SessionId);
        var result = await mediator.Send(query, cancellationToken);

        return new GetCurrentUserDetailsResponse
        {
            UserId = result.UserId.ToString(),
        };
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<LogoutUserResponse> LogoutUser(CancellationToken cancellationToken)
    {
        var command = new LogoutUser.Query(user.SessionId);
        _ = await mediator.Send(command, cancellationToken);

        return new LogoutUserResponse();
    }
}