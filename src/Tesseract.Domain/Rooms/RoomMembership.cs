using Tesseract.Domain.Common;
using Tesseract.Domain.Events;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Rooms;

public class RoomMembership(RoomId roomId, UserId userId)
    : AggregateRoot<RoomMembershipId, Event>(new RoomMembershipId(roomId, userId))
{
    public RoomId RoomId { get; } = roomId;
    public UserId UserId { get; } = userId;

    public static RoomMembership Create(Room room, User user)
    {
        var membership = new RoomMembership(room.Id, user.Id);

        membership.RaiseEvent(new JoinRoomEvent(room, user));

        return membership;
    }
}