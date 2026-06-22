using Tesseract.Domain.Events;
using Tesseract.Infrastructure.ClientServer.Events.Dao;

namespace Tesseract.Infrastructure.ClientServer.Events.Mappers;

internal class MessageRoomEventMapper : EventMapper<MessageRoomEvent, MessageRoomEventDao>
{
    public override string Type => EventTypes.MessageRoom;

    protected override MessageRoomEventDao ToDao(MessageRoomEvent typedEvent) => new()
    {
        EventId = typedEvent.Id.Value,
        RoomHandle = typedEvent.Room.Handle.ToString(),
        SenderHandle = typedEvent.Sender.Handle.ToString(),
        Timestamp = typedEvent.Timestamp,
        StateKey = typedEvent.StateKey,
        Content = new MessageRoomContent
        {
            Body = typedEvent.Body,
        },
    };
}