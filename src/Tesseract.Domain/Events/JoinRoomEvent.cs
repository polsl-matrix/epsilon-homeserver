using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Events;

public class JoinRoomEvent(Room room, User user, UserHandle userHandle, VDomain domain)
    : MemberRoomEvent(room, user, user, userHandle, RoomMembershipState.Join, domain);