using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Rooms.Contracts;

public sealed class GetMembersResponse
{
    [JsonPropertyName("chunk")]
    public required JsonDocument[] Chunk { get; init; }
}