using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Discovery.Contracts;

public sealed class GetSupportedVersionsResponse
{
    [JsonPropertyName("versions")]
    public required IReadOnlyCollection<string> Versions { get; init; }
}