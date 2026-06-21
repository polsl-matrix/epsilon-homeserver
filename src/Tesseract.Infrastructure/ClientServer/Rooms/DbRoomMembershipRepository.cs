using Dapper;
using Tesseract.Domain.Rooms;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Rooms;

internal class DbRoomMembershipRepository(IDbConnectionFactory dbConnectionFactory) : IRoomMembershipRepository
{
    public async Task SaveAsync(RoomMembership roomMembership, CancellationToken cancellationToken)
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
}