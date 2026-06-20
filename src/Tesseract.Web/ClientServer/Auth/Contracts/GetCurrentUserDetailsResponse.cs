using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class GetCurrentUserDetailsResponse
{
    [JsonPropertyName("device_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeviceId { get; init; }

    [JsonPropertyName("is_guest")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsGuest { get; init; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }
}