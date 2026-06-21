using Dapper;
using System.Text.Json;
using Tesseract.Application.ClientServer.Rooms;
using Tesseract.Domain;
using Tesseract.Infrastructure.ClientServer.Events;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Rooms;

internal class DbEventRepository : IEventRepository
{
    private readonly Dictionary<string, IEventMapper> _roomEventMappers;

    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DbEventRepository(IEnumerable<IEventMapper> roomEventMappers,
        IDbConnectionFactory dbConnectionFactory)
    {
        _roomEventMappers = roomEventMappers.ToDictionary(mapper => mapper.Type);
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InsertAsync(Event @event, CancellationToken cancellationToken)
    {
        await using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO chat.room_events(event_id, room_id, timestamp, payload)
                           VALUES (@EventId, @RoomId, @Timestamp, @Payload);
                           """;

        var parameters = new
        {
            EventId = @event.Id.Value,
            RoomId = @event.RoomId.Value,
            Timestamp = @event.Timestamp,
            Payload = SerializePayload(@event),
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    private string SerializePayload(Event @event)
    {
        if (!_roomEventMappers.TryGetValue(@event.Type, out var roomEventMapper))
        {
            throw new ArgumentException(@event.Type);
        }

        var payload = roomEventMapper.ToDao(@event);
        return JsonSerializer.Serialize(payload);
    }
}