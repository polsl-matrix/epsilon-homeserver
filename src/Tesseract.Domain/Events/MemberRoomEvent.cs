using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Events;

public class MemberRoomEvent(
    Room room,
    User sender,
    User target,
    UserHandle targetHandle,
    RoomMembershipState state,
    VDomain domain)
    : Event(EventTypes.MemberRoom, room, sender, targetHandle.ToString(), domain)
{
    public User Target { get; } = target;
    public RoomMembershipState Membership { get; } = state;
}