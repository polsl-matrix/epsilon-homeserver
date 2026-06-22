using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Profile.Contracts;

public sealed class GetAvatarUrlResponse
{
    [JsonPropertyName("avatar_url")]
    public required string AvatarUrl { get; init; }
}