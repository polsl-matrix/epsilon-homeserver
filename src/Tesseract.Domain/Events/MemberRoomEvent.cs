using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class MemberRoomEvent(EventHandle handle, Room room, User target, RoomMembershipState state, User sender)
    : Event(handle, EventTypes.MemberRoom, room, sender, target.Handle.ToString())
{
    public required RoomMembershipState Membership { get; init; } = state;
}