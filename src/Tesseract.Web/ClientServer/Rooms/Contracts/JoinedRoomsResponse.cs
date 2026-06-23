using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Rooms.Contracts;

public sealed class JoinedRoomsResponse
{
    [JsonPropertyName("joined_rooms")]
    public required IReadOnlyList<string> JoinedRoomHandles { get; init; }
}