using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tesseract.Application.ClientServer.Rooms;
using Tesseract.Web.ClientServer.Rooms.Contracts;

namespace Tesseract.Web.ClientServer.Rooms;

[ApiController]
// TODO: Enable rate limiting
[Route("_matrix/client")]
public class RoomController(IMediator mediator)
{
    [HttpPost("v3/createRoom")]
    public async Task<CreateRoomResponse> CreateRoom(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRoom.Command();
        var result = await mediator.Send(command, cancellationToken);

        return new CreateRoomResponse
        {
            RoomId = result.Handle.ToString(),
        };
    }
}