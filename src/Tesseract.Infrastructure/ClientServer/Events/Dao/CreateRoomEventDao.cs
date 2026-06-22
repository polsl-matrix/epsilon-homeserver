using System.Text.Json.Serialization;

namespace Tesseract.Infrastructure.ClientServer.Events.Dao;

public class CreateRoomEventDao : EventDao
{
    [JsonPropertyName("content")]
    public required CreateRoomContent Content { get; init; }
}

public class CreateRoomContent;