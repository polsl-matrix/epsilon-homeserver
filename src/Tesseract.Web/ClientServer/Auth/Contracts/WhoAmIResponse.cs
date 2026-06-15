using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class WhoAmIResponse
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }
}