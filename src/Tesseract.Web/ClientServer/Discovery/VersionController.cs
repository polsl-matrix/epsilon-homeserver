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
[Route("_matrix/client")]
public class VersionController(IMediator mediator)
{
    [HttpGet("versions")]
    public async Task<GetSupportedVersionsResponse> GetSupportedVersions(CancellationToken cancellationToken)
    {
        var result = await mediator
            .Send(new GetSupportedVersions.Query(), cancellationToken);

        return new GetSupportedVersionsResponse
        {
            Versions = result.Versions,
        };
    }
}