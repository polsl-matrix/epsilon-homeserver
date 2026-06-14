using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class RegisterAccountRequest
{
    [JsonPropertyName("auth")]
    public AuthenticationData? Auth { get; init; }

    [JsonPropertyName("username")]
    public string? Username { get; init; }

    [JsonPropertyName("password")]
    public string? Password { get; init; }

    [JsonPropertyName("device_id")]
    public string? DeviceId { get; init; }

    [JsonPropertyName("initial_device_display_name")]
    public string? InitialDeviceDisplayName { get; init; }

    [JsonPropertyName("inhibit_login")]
    public bool InhibitLogin { get; init; }

    [JsonPropertyName("refresh_token")]
    public bool RefreshToken { get; init; }

    public sealed class AuthenticationData
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("session")]
        public string? Session { get; init; }
    }
}