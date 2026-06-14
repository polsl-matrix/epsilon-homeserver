using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tesseract.Domain.Common.Constants;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class RegisterAccountRequest
{
    [JsonPropertyName("password")]
    public required string Password { get; init; }

    [JsonPropertyName("username")]
    [RegularExpression(Patterns.Localpart)]
    public required string Username { get; init; }

    public sealed class AuthenticationData
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("session")]
        public string? Session { get; init; }
    }
}