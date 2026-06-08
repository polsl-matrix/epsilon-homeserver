using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Web.ClientServer.Discovery.Contracts;

namespace Tesseract.Web.ClientServer.Discovery;

[ApiController]
[DisableRateLimiting]
[AllowAnonymous]
[Route(".well-known/matrix")]
public class WellKnownController(IMediator mediator) : ControllerBase
{
    [HttpGet("client")]
    public async Task<ActionResult<GetDomainDiscoveryResponse>> GetDomainDiscovery(CancellationToken cancellationToken)
    {
        var result = await mediator
            .Send(new GetDomainDiscovery.Query(), cancellationToken);

        var response = new GetDomainDiscoveryResponse
        {
            Homeserver = new GetDomainDiscoveryResponse.HomeserverInfo
            {
                BaseUrl = result.HomeserverBaseUrl,
            },
        };

        if (result.IdentityServerBaseUrl is not null)
        {
            response.IdentityServer = new GetDomainDiscoveryResponse.IdentityServerInfo
            {
                BaseUrl = result.IdentityServerBaseUrl,
            };
        }

        return response;
    }
}