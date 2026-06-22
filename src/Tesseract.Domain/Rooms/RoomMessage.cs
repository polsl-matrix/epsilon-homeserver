using Tesseract.Domain.Common;
using Tesseract.Domain.Events;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Rooms;

public class RoomMessage(RoomId roomId, UserId senderId, string body)
    : AggregateRoot<RoomMessageId, Event>(new RoomMessageId())
{
    public RoomId RoomId { get; } = roomId;
    public UserId UserId { get; } = senderId;
    public string Body { get; } = body;

    public static RoomMessage Create(Room room, User sender, string body)
    {
        var message = new RoomMessage(room.Id, sender.Id, body);
        var domain = room.Handle.Domain;

        message.RaiseEvent(MessageRoomEvent.Create(room, sender, body, domain));

        return message;
    }
}