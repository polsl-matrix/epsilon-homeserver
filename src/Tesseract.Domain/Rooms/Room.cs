using Tesseract.Domain.Common;
using Tesseract.Domain.Events;
using Tesseract.Domain.Users;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Rooms;

public class Room(RoomId id, RoomHandle handle) : AggregateRoot<RoomId, Event>(id)
{
    public RoomHandle Handle { get; } = handle;

    public static Room Create(User creator, VDomain domain)
    {
        var handle = RoomHandle.Random(domain.Value);
        var room = new Room(RoomId.Random(), handle);

        room.RaiseEvent(new CreateRoomEvent(room, creator));

        return room;
    }
}