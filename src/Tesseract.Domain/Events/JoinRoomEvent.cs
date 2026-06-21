using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class JoinRoomEvent(Room room, User user)
    : MemberRoomEvent(room, user, RoomMembershipState.Join, user);