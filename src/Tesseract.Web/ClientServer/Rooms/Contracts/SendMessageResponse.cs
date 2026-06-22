using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Rooms.Contracts;

public sealed class SendMessageResponse
{
    [JsonPropertyName("event_id")]
    public required string EventHandle { get; init; }
}