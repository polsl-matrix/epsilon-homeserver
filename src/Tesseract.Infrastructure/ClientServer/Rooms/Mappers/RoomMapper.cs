using Tesseract.Domain.Rooms;
using Tesseract.Infrastructure.ClientServer.Rooms.Dao;

namespace Tesseract.Infrastructure.ClientServer.Rooms.Mappers;

internal static class RoomMapper
{
    public static Room ToDomain(this RoomDao dao)
    {
        var roomId = new RoomId(dao.RoomId);
        var handle = new RoomHandle(dao.Localpart, dao.Domain);

        return new Room(roomId, handle);
    }
}