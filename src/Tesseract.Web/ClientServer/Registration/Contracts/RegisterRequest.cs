using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Registration.Contracts;

public sealed class RegisterRequest
{
    [JsonPropertyName("auth")]
    public AuthenticationData? Auth { get; init; }

    [JsonPropertyName("device_id")]
    public string? DeviceId { get; init; }

    [JsonPropertyName("inhibit_login")]
    public bool InhibitLogin { get; init; }

    [JsonPropertyName("initial_device_display_name")]
    public string? InitialDeviceDisplayName { get; init; }

    [JsonPropertyName("password")]
    public string? Password { get; init; }

    [JsonPropertyName("refresh_token")]
    public bool RefreshToken { get; init; }

    [JsonPropertyName("username")]
    public string? Username { get; init; }

    public sealed class AuthenticationData
    {
        [JsonPropertyName("session")]
        public string? Session { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }
}