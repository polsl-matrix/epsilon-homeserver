using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class AuthenticateUserResponse
{
    [JsonPropertyName("user_id")]
    public required string Handle { get; init; }
}