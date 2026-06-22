using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Web.ClientServer.Profile.Contracts;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.ClientServer.Profile;

[ApiController]
// TODO: Add rate limiter.
[Route("_matrix/client/v3/profile/{userHandle}")]
public sealed class ProfileController(ISender sender, ICurrentUser user) : ControllerBase
{
    [Authorize]
    [HttpPut("displayname")]
    public async Task<UpdateDisplayNameResponse> UpdateDisplayName(UpdateDisplayNameRequest request, string userHandle,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDisplayName.Command(user.Id, userHandle, request.DisplayName);
        _ = await sender.Send(command, cancellationToken);

        return new UpdateDisplayNameResponse();
    }

    [HttpPut("avatar_url")]
    [Authorize]
    public async Task<UpdateAvatarUrlResponse> UpdateAvatarUrl(UpdateAvatarUrlRequest request, string userHandle,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAvatarUrl.Command(user.Id, userHandle, request.AvatarUrl);
        _ = await sender.Send(command, cancellationToken);

        return new UpdateAvatarUrlResponse();
    }
}