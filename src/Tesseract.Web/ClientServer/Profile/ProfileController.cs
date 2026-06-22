using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Web.ClientServer.Profile.Contracts;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.ClientServer.Profile;

[ApiController]
[Route("_matrix/client")]
public sealed class ProfileController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    [HttpPut("v3/profile/{userId}/avatar_url")]
    [Authorize]
    public async Task<object> UpdateAvatarUrl(
        string userId,
        UpdateAvatarUrlRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAvatarUrl.Command(
            currentUser.Id,
            userId,
            request.AvatarUrl);

        await sender.Send(command, cancellationToken);

        return new { };
    }
}