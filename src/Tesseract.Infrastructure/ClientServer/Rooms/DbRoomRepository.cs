using Dapper;
using Tesseract.Application.ClientServer.Rooms;
using Tesseract.Domain.Rooms;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Rooms;

public class DbRoomRepository(IDbConnectionFactory dbConnectionFactory) : IRoomRepository
{
    public async Task Save(Room room)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO chat.rooms(room_id, localpart, domain)
                           VALUES (@RoomId, @Localpart, @Domain);
                           """;

        var parameters = new
        {
            RoomId = room.Id.Value,
            Localpart = room.Handle.Localpart.Value,
            Domain = room.Handle.Domain.Value,
        };

        await connection.ExecuteAsync(sql, parameters);
    }
}