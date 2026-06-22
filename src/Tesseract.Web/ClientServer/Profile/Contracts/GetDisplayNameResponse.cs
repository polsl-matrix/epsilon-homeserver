using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Profile.Contracts;

public sealed class GetDisplayNameResponse
{
    [JsonPropertyName("displayname")]
    public required string DisplayName { get; init; }
}