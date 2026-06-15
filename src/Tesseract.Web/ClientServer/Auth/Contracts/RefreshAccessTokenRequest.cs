using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class RefreshAccessTokenRequest
{
    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }
}