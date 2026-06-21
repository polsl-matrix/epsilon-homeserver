using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Events;

public enum Membership
{
    Join,
}

[method: SetsRequiredMembers]
public class MemberRoomEvent(Room room, User sender, User target, Membership membership)
    : Event(EventTypes.MemberRoom, room, sender, target.Handle.ToString())
{
    public required Membership Membership { get; init; }
}