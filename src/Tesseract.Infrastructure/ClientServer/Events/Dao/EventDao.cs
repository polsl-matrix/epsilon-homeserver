using System.Text.Json.Serialization;
using Tesseract.Domain.Events;

namespace Tesseract.Infrastructure.ClientServer.Events.Dao;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CreateRoomEventDao), EventTypes.CreateRoom)]
[JsonDerivedType(typeof(MemberRoomEventDao), EventTypes.MemberRoom)]
[JsonDerivedType(typeof(MessageRoomEventDao), EventTypes.MessageRoom)]
public class EventDao
{
    [JsonPropertyName("event_id")]
    public required string EventHandle { get; init; }

    [JsonPropertyName("room_id")]
    public required string RoomHandle { get; init; }

    [JsonPropertyName("sender")]
    public required string SenderHandle { get; init; }

    [JsonPropertyName("origin_server_ts")]
    public required DateTime Timestamp { get; init; }

    [JsonPropertyName("state_key")]
    public string? StateKey { get; init; }
}