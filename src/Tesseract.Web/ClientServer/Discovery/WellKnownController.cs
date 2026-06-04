using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Web.ClientServer.Discovery.Contracts;

namespace Tesseract.Web.ClientServer.Discovery;

[ApiController]
[DisableRateLimiting]
[AllowAnonymous]
[Route(".well-known/matrix")]
public class WellKnownController(IMediator mediator)
{
    [HttpGet("support")]
    public async Task<GetSupportInfoResponse> GetSupportInfo(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSupportInfo.Query(), cancellationToken);
        return new GetSupportInfoResponse
        {
            SupportPage = result.SupportInfo.SupportPage,
            Contacts =  result.SupportInfo.Contacts,
        };
    }
}