using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using Tesseract.Infrastructure.ClientServer.Rooms.Dao;

namespace Tesseract.Infrastructure.ClientServer.Rooms.Mappers;

internal static class RoomMembershipMapper
{
    public static RoomMembership ToDomain(this RoomMembershipDao dao)
    {
        var userId = new UserId(dao.UserId);
        var roomId = new RoomId(dao.RoomId);

        return new RoomMembership(roomId, userId);
    }
}