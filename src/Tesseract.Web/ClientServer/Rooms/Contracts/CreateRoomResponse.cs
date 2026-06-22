using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Rooms.Contracts;

public sealed class CreateRoomResponse
{
    [JsonPropertyName("room_id")]
    public required string RoomId { get; init; }
}