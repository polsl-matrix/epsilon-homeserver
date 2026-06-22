using Dapper;
using Tesseract.Application.ClientServer.Rooms.Abstractions;
using Tesseract.Domain.Rooms;
using Tesseract.Infrastructure.ClientServer.Rooms.Dao;
using Tesseract.Infrastructure.ClientServer.Rooms.Mappers;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Rooms;

internal class DbRoomRepository(IDbConnectionFactory dbConnectionFactory) : IRoomRepository
{
    public async Task InsertAsync(Room room, CancellationToken _)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

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

    public async Task<Room?> GetByHandleAsync(RoomHandle handle, CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                            SELECT room_id   {nameof(RoomDao.RoomId)},
                                   localpart {nameof(RoomDao.Localpart)},
                                   domain    {nameof(RoomDao.Domain)}
                            FROM chat.rooms
                            WHERE localpart = @Localpart
                              AND domain = @Domain;
                            """;

        var parameters = new
        {
            Localpart = handle.Localpart.Value,
            Domain = handle.Domain.Value,
        };

        if (await connection.QuerySingleOrDefaultAsync<RoomDao>(sql, parameters) is not { } roomDao)
        {
            return null;
        }

        return roomDao.ToDomain();
    }
}