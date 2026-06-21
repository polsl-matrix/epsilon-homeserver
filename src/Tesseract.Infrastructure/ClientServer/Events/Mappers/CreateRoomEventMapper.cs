using Tesseract.Domain.Events;
using Tesseract.Infrastructure.ClientServer.Events.Dao;

namespace Tesseract.Infrastructure.ClientServer.Events.Mappers;

internal class CreateRoomEventMapper : EventMapper<CreateRoomEvent, CreateRoomEventDao>
{
    public override string Type => EventTypes.CreateRoom;

    protected override CreateRoomEventDao ToDao(CreateRoomEvent typedEvent) => new()
    {
        EventId = typedEvent.Id.Value,
        RoomHandle = typedEvent.Room.Handle.ToString(),
        SenderHandle = typedEvent.Sender.Handle.ToString(),
        Timestamp = typedEvent.Timestamp,
        StateKey = typedEvent.StateKey,
        Content = new Content(),
    };
}