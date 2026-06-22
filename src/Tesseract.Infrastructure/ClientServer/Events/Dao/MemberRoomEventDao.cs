using System.Text.Json.Serialization;

namespace Tesseract.Infrastructure.ClientServer.Events.Dao;

public class MemberRoomEventDao : EventDao
{
    [JsonPropertyName("content")]
    public required MemberRoomContent Content { get; init; }
}

public class MemberRoomContent
{
    [JsonPropertyName("membership")]
    public required string Membership { get; init; }
}