using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Web.ClientServer.Profile.Contracts;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.ClientServer.Profile;

[ApiController]
[Route("_matrix/client")]
public sealed class ProfileController(ISender sender, ICurrentUser user) : ControllerBase
{
    [HttpPut("v3/profile/{userHandle}/avatar_url")]
    [Authorize]
    public async Task<UpdateAvatarUrlResponse> UpdateAvatarUrl(UpdateAvatarUrlRequest request, string userHandle,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAvatarUrl.Command(user.Id, userHandle, request.AvatarUrl);
        _ = await sender.Send(command, cancellationToken);

        return new UpdateAvatarUrlResponse();
    }
}