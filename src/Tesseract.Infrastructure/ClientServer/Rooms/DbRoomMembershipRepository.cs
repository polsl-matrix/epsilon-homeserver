using Dapper;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using Tesseract.Infrastructure.ClientServer.Rooms.Dao;
using Tesseract.Infrastructure.ClientServer.Rooms.Mappers;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Rooms;

internal class DbRoomMembershipRepository(IDbConnectionFactory dbConnectionFactory) : IRoomMembershipRepository
{
    public async Task InsertAsync(RoomMembership roomMembership, CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO chat.room_memberships(room_id, user_id)
                           VALUES (@RoomId, @UserId);
                           """;

        var parameters = new
        {
            RoomId = roomMembership.RoomId.Value,
            UserId = roomMembership.UserId.Value,
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<RoomMembership?> GetByRoomIdAndUserIdAsync(RoomId roomId, UserId userId,
        CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                            SELECT room_id {nameof(RoomMembershipDao.RoomId)},
                                   user_id {nameof(RoomMembershipDao.UserId)}
                            FROM chat.room_memberships
                            WHERE room_id = @RoomId
                              AND user_id = @UserId;
                            """;

        var parameters = new
        {
            RoomId = roomId.Value,
            UserId = userId.Value,
        };

        if (await connection.QuerySingleOrDefaultAsync<RoomMembershipDao>(sql, parameters) is not { } membershipDao)
        {
            return null;
        }

        return membershipDao.ToDomain();
    }

    public async Task<IEnumerable<Room>> GetRoomsByUserIdAsync(UserId userId,
        CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                            SELECT r.room_id   {nameof(RoomDao.RoomId)},
                                   r.localpart {nameof(RoomDao.Localpart)},
                                   r.domain    {nameof(RoomDao.Domain)}
                            FROM chat.room_memberships m
                                JOIN chat.rooms r ON m.room_id = r.room_id
                            WHERE m.user_id = @UserId;
                            """;

        var parameters = new
        {
            UserId = userId.Value,
        };

        var roomDaos = await connection.QueryAsync<RoomDao>(sql, parameters);

        return roomDaos.Select(roomDao => roomDao.ToDomain());
    }
}