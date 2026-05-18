using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Discovery.Contracts;

public sealed class IdentityServerInfoResponse
{
    [JsonPropertyName("base_url")]
    public required string BaseUrl { get; init; }
}