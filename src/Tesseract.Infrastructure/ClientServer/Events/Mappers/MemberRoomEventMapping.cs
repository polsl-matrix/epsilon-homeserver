using Tesseract.Domain.Events;
using Tesseract.Domain.Rooms;
using Tesseract.Infrastructure.ClientServer.Events.Dao;

namespace Tesseract.Infrastructure.ClientServer.Events.Mappers;

internal class MemberRoomEventMapper : EventMapper<MemberRoomEvent, MemberRoomEventDao>
{
    public override string Type => EventTypes.MemberRoom;

    protected override MemberRoomEventDao ToDao(MemberRoomEvent typedEvent) => new()
    {
        EventId = typedEvent.Id.Value,
        RoomHandle = typedEvent.Room.Handle.ToString(),
        SenderHandle = typedEvent.Sender.Handle.ToString(),
        Timestamp = typedEvent.Timestamp,
        StateKey = typedEvent.StateKey,
        Content = new MemberRoomContent
        {
            Membership = GetMembershipText(typedEvent.Membership),
        },
    };

    private static string GetMembershipText(RoomMembershipState membership) => membership switch
    {
        RoomMembershipState.Join => "join",
        _ => throw new ArgumentException($"Unknown membership: {membership}"),
    };
}