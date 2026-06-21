using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tesseract.Application.ClientServer.Rooms;
using Tesseract.Web.ClientServer.Rooms.Contracts;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.ClientServer.Rooms;

[ApiController]
// TODO: Add rate limiter.
[Route("_matrix/client")]
public class RoomController(IMediator mediator, ICurrentUser user)
{
    [HttpPost("v3/createRoom")]
    public async Task<CreateRoomResponse> CreateRoom(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRoom.Command(user.Id);
        var result = await mediator.Send(command, cancellationToken);

        return new CreateRoomResponse
        {
            RoomId = result.Handle.ToString(),
        };
    }
}