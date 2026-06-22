using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class CreateRoomEvent(EventHandle handle, Room room, User creator)
    : Event(handle, EventTypes.CreateRoom, room, creator, string.Empty)
{
    public static CreateRoomEvent Create(Room room, User creator, VDomain domain)
    {
        var handle = EventHandle.Random(domain.Value);

        return new CreateRoomEvent(handle, room, creator);
    }
}