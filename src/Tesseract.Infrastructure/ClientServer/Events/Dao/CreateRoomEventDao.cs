using System.Text.Json.Serialization;

namespace Tesseract.Infrastructure.ClientServer.Events.Dao;

public class CreateRoomEventDao : EventDao
{
    [JsonPropertyName("content")]
    public required Content Content { get; init; }
}

public class Content;