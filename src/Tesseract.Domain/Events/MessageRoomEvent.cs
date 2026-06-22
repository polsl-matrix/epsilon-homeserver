using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class MessageRoomEvent(EventHandle handle, Room room, User sender, string body)
    : Event(handle, EventTypes.MessageRoom, room, sender, null)
{
    public required string Body { get; init; } = body;

    public static MessageRoomEvent Create(Room room, User creator, string body, VDomain domain)
    {
        var handle = EventHandle.Random(domain.Value);
        return new MessageRoomEvent(handle, room, creator, body);
    }
}