using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tesseract.Application.ClientServer.Registration;
using Tesseract.Web.ClientServer.Registration.Contracts;

namespace Tesseract.Web.ClientServer.Registration;

[ApiController]
[AllowAnonymous]
[Route("_matrix/client/v3")]
public class RegisterController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        [FromQuery] string kind = "user",
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterAccount.Command(
            kind,
            request.Username,
            request.Password,
            request.DeviceId,
            request.InitialDeviceDisplayName,
            request.InhibitLogin,
            request.Auth is null
                ? null
                : new RegisterAccount.AuthenticationData(request.Auth.Type, request.Auth.Session));

        var result = await mediator.Send(command, cancellationToken);

        return result switch
        {
            RegisterAccount.Response.Registered registered => new OkObjectResult(
                new RegisterResponse
                {
                    UserId = registered.UserId,
                    DeviceId = registered.DeviceId,
                    AccessToken = registered.AccessToken,
                }),

            RegisterAccount.Response.AuthenticationRequired authentication => new ObjectResult(
                new UserInteractiveAuthResponse
                {
                    Session = authentication.Session,
                    Flows = authentication.Flows
                        .Select(stages => new UserInteractiveAuthResponse.FlowInformation { Stages = stages })
                        .ToList(),
                })
            {
                StatusCode = StatusCodes.Status401Unauthorized,
            },

            _ => throw new InvalidOperationException($"Unexpected response type: {result.GetType().Name}."),
        };
    }
}