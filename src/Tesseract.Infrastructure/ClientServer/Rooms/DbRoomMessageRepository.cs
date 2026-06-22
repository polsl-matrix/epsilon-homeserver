using Dapper;
using Tesseract.Application.ClientServer.Rooms.Abstractions;
using Tesseract.Domain.Rooms;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Rooms;

internal class DbRoomMessageRepository(IDbConnectionFactory dbConnectionFactory) : IRoomMessageRepository
{
    public async Task InsertAsync(RoomMessage message, CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO chat.room_messages(message_id, room_id, user_id, body)
                           VALUES (@MessageId, @RoomId, @UserId, @Body);
                           """;

        var parameters = new
        {
            MessageId = message.Id.Value,
            RoomId = message.RoomId.Value,
            UserId = message.UserId.Value,
            Body = message.Body,
        };

        await connection.ExecuteAsync(sql, parameters);
    }
}