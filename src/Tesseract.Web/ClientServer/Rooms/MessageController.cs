using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using Tesseract.Application.ClientServer.Rooms;
using Tesseract.Domain.Events;
using Tesseract.Web.ClientServer.Rooms.Contracts;
using Tesseract.Web.Common;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.ClientServer.Rooms;

[ApiController]
[DisableRateLimiting]
[Authorize]
[Route("_matrix/client/v3/rooms/{roomHandle}")]
public class MessageController(IMediator mediator, ICurrentUser user)
{
    [Idempotent]
    [HttpPut("send/" + EventTypes.MessageRoom + "/{txnId}")]
    public async Task<SendMessageResponse> SendMessage(SendMessageRequest request, string roomHandle, string txnId,
        CancellationToken cancellationToken)
    {
        var command = new SendMessage.Command(roomHandle, user.Id.Value, request.Body);
        var result = await mediator.Send(command, cancellationToken);

        return new SendMessageResponse
        {
            EventHandle = result.EventHandle.ToString(),
        };
    }

    [HttpGet("messages")]
    public async Task<GetMessagesResponse> GetMessages(GetMessagesRequest request, string roomHandle,
        CancellationToken cancellationToken)
    {
        var query = new GetMessages.Query(roomHandle, user.Id.Value);
        var result = await mediator.Send(query, cancellationToken);

        return new GetMessagesResponse
        {
            Chunk = result.Events.Select(@event => JsonDocument.Parse(@event)).ToArray(),
        };
    }
}