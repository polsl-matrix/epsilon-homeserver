using System.Text.Json.Serialization;

namespace Tesseract.Infrastructure.ClientServer.Events.Dao;

public class MessageRoomEventDao : EventDao
{
    [JsonPropertyName("content")]
    public required MessageRoomContent Content { get; init; }
}

public class MessageRoomContent
{
    [JsonPropertyName("membership")]
    public required string Body { get; init; }
}