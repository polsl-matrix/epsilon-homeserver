using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Profile.Contracts;

public sealed class GetUserProfileResponse
{
    [JsonPropertyName("avatar_url")]
    public required string? AvatarUrl { get; init; }

    [JsonPropertyName("displayname")]
    public required string? DisplayName { get; init; }
}