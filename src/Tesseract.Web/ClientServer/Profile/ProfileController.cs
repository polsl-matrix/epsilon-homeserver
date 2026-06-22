using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Web.ClientServer.Profile.Contracts;

namespace Tesseract.Web.ClientServer.Profile;

[ApiController]
[Route("_matrix/client")]
public sealed class ProfileController(ISender sender) : ControllerBase
{
    [HttpGet("v3/profile/{userId}/avatar_url")]
    [AllowAnonymous]
    public async Task<GetAvatarUrlResponse> GetAvatarUrl(
        string userId,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetAvatarUrl.Query(userId), cancellationToken);

        return new GetAvatarUrlResponse { AvatarUrl = response.AvatarUrl };
    }
}