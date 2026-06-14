using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class GetSupportedAuthenticationFlowsResponse
{
    [JsonPropertyName("flows")]
    public required IReadOnlyList<LoginFlow> Flows { get; init; }
}

public sealed class LoginFlow
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }
}