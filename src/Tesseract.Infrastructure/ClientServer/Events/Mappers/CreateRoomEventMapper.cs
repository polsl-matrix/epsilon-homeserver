using Tesseract.Domain;
using Tesseract.Domain.Events;
using Tesseract.Infrastructure.ClientServer.Events.Dao;

namespace Tesseract.Infrastructure.ClientServer.Events.Mappers;

internal class CreateRoomEventMapper : EventMapper<CreateRoomEvent, CreateRoomEventDao>
{
    public override string Type => EventTypes.CreateRoom;

    protected override CreateRoomEventDao ToDao(CreateRoomEvent typedEvent) => new()
    {
        EventId = typedEvent.Id.Value,
        RoomId = typedEvent.RoomId.Value,
        SenderId = typedEvent.SenderId.Value,
        Timestamp = typedEvent.Timestamp,
        StateKey = typedEvent.StateKey,
        Content = new Content(),
    };
}