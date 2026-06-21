using System.Text.Json.Serialization;

namespace Tesseract.Infrastructure.ClientServer.Events.Dao;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
public class EventDao
{
    [JsonPropertyName("event_id")]
    public required Guid EventId { get; init; }

    [JsonPropertyName("room_id")]
    public required Guid RoomId { get; init; }

    [JsonPropertyName("sender_id")]
    public required Guid SenderId { get; init; }

    [JsonPropertyName("origin_server_ts")]
    public required DateTime Timestamp { get; init; }

    [JsonPropertyName("state_key")]
    public string? StateKey { get; init; }
}