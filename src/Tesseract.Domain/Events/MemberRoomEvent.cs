using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class MemberRoomEvent(Room room, User target, RoomMembershipState state, User sender)
    : Event(EventTypes.MemberRoom, room, sender, target.Handle.ToString())
{
    public required RoomMembershipState Membership { get; init; } = state;
}