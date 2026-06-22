using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class JoinRoomEvent(EventHandle handle, Room room, User user)
    : MemberRoomEvent(handle, room, user, RoomMembershipState.Join, user)
{
    public static JoinRoomEvent Create(Room room, User user, VDomain domain)
    {
        var handle = EventHandle.Random(domain.Value);

        return new JoinRoomEvent(handle, room, user);
    }
}