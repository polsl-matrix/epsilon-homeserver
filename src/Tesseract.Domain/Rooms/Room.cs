using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Rooms;

public class Room(RoomId id, RoomHandle handle) : AggregateRoot<RoomId, DomainEvent>(id)
{
    public RoomHandle Handle { get; } = handle;

    public static Room Create(VDomain domain)
    {
        var handle = RoomHandle.Random(domain.Value);
        return new Room(RoomId.Random(), handle);
    }
}