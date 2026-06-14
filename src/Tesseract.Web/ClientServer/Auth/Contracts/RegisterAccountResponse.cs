using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class RegisterAccountResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }
}