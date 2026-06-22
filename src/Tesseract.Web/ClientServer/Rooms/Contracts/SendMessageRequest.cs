using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Rooms.Contracts;

public sealed class SendMessageRequest
{
    [JsonPropertyName("body")]
    public required string Body { get; init; }
}