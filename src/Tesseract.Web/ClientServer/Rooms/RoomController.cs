using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Tesseract.Application.ClientServer.Rooms;
using Tesseract.Web.ClientServer.Rooms.Contracts;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.ClientServer.Rooms;

[ApiController]
// TODO: Add rate limiter.
[Route("_matrix/client/v3")]
public class RoomController(IMediator mediator, ICurrentUser user)
{
    [HttpPost("createRoom")]
    public async Task<CreateRoomResponse> CreateRoom(CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoom.Command(user.Id);
        var result = await mediator.Send(command, cancellationToken);

        return new CreateRoomResponse
        {
            RoomId = result.Handle.ToString(),
        };
    }

    [HttpPost("rooms/{roomHandle}/join")]
    public async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, string roomHandle,
        CancellationToken cancellationToken)
    {
        var command = new JoinRoom.Command(roomHandle, user.Id);
        var result = await mediator.Send(command, cancellationToken);

        return new JoinRoomResponse
        {
            RoomHandle = result.Handle.ToString(),
        };
    }

    [HttpGet("joined_rooms")]
    public async Task<JoinedRoomsResponse> GetJoinedRooms(CancellationToken cancellationToken)
    {
        var query = new GetJoinedRooms.Query(user.Id);
        var result = await mediator.Send(query, cancellationToken);

        return new JoinedRoomsResponse
        {
            JoinedRoomHandles = [.. result.RoomHandles.Select(rh => rh.ToString())],
        };
    }

    [HttpGet("rooms/{roomHandle}/members")]
    public async Task<GetMembersResponse> GetMembers(GetMembersRequest request, string roomHandle,
        CancellationToken cancellationToken)
    {
        var query = new GetMembers.Query(roomHandle, user.Id);
        var result = await mediator.Send(query, cancellationToken);

        return new GetMembersResponse
        {
            Chunk = result.Events.Select(@event => JsonDocument.Parse(@event)).ToArray(),
        };
    }

    [HttpGet("rooms/{roomHandle}/state")]
    public async Task<GetStateResponse> GetState(GetStateRequest request, string roomHandle,
        CancellationToken cancellationToken)
    {
        var query = new GetState.Query(roomHandle, user.Id);
        var result = await mediator.Send(query, cancellationToken);

        return new GetStateResponse
        {
            Chunk = result.Events.Select(@event => JsonDocument.Parse(@event)).ToArray(),
        };
    }
}