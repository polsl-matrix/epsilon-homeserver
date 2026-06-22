using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Rooms.Contracts;

public sealed class GetMessagesResponse
{
    [JsonPropertyName("chunk")]
    public required JsonDocument[] Chunk { get; init; }
}