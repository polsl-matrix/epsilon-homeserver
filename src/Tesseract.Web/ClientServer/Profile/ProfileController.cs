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
    [HttpPut("v3/profile/{userId}/displayname")]
    [Authorize]
    public async Task<object> UpdateDisplayName(
        string userId,
        UpdateDisplayNameRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDisplayName.Command(
            currentUser.Id,
            userId,
            request.DisplayName);

        await sender.Send(command, cancellationToken);

        return new { };
    }
}