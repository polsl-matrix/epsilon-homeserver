using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Web.ClientServer.Profile.Contracts;

namespace Tesseract.Web.ClientServer.Profile;

[ApiController]
[Route("_matrix/client")]
public class ProfileController(IMediator mediator)
{
    [HttpGet("v3/profile/{userId}/displayname")]
    [AllowAnonymous]
    public async Task<GetDisplayNameResponse> GetDisplayName(
        string userId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDisplayName.Query(userId), cancellationToken);

        return new GetDisplayNameResponse
        {
            DisplayName = result.DisplayName,
        };
    }
}