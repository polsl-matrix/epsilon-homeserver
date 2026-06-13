using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class AuthenticateUserRequest
{
    [JsonPropertyName("identifier")]
    public required UserIdentifier Identifier { get; init; }

    [JsonPropertyName("password")]
    public string? Password { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    public sealed class UserIdentifier
    {
        [JsonPropertyName("user")]
        public string? User { get; init; }
    }
}