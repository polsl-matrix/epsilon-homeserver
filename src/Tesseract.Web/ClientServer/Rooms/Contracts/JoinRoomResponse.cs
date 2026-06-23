using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Rooms.Contracts;

public sealed class JoinRoomResponse
{
    [JsonPropertyName("room_id")]
    public required string RoomHandle { get; init; }
}