using Dapper;
using System.Text.Json;
using Tesseract.Application.ClientServer.Rooms.Abstractions;
using Tesseract.Domain.Events;
using Tesseract.Domain.Rooms;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Events;

internal class DbEventRepository : IEventRepository
{
    private readonly Dictionary<string, IEventMapper> _eventMappers;

    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DbEventRepository(IEnumerable<IEventMapper> eventMappers,
        IDbConnectionFactory dbConnectionFactory)
    {
        _eventMappers = eventMappers.ToDictionary(mapper => mapper.Type);
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InsertAsync(Event @event, CancellationToken cancellationToken)
    {
        await using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO chat.room_events(event_id, room_id, timestamp, payload, event_type, state_key)
                           VALUES (@EventId, @RoomId, @Timestamp, @Payload, @EventType, @StateKey);
                           """;

        var parameters = new
        {
            EventId = @event.Id.Value,
            EventType = @event.Type,
            RoomId = @event.Room.Id.Value,
            Timestamp = @event.Timestamp,
            StateKey = @event.StateKey,
            Payload = SerializePayload(@event),
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<IEnumerable<string>> GetByRoomIdAsync(RoomId roomId, CancellationToken cancellationToken)
    {
        await using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
                           SELECT payload
                           FROM chat.room_events
                           WHERE room_id = @RoomId
                           ORDER BY timestamp;
                           """;

        var parameters = new
        {
            RoomId = roomId.Value,
        };

        return await connection.QueryAsync<string>(sql, parameters);
    }

    private string SerializePayload(Event @event)
    {
        if (!_eventMappers.TryGetValue(@event.Type, out var eventMapper))
        {
            throw new ArgumentException(@event.Type);
        }

        var payload = eventMapper.ToDao(@event);
        return JsonSerializer.Serialize(payload);
    }
}