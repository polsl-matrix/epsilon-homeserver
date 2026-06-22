using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Events;

public class MessageRoomEvent(Room room, User sender, string body, VDomain domain)
    : Event(EventTypes.MessageRoom, room, sender, null, domain)
{
    public string Body { get; } = body;
}