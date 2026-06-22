using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Profile.Contracts;

public sealed class UpdateDisplayNameRequest
{
    [JsonPropertyName("displayname")]
    public string? DisplayName { get; init; }
}