using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class UserInteractiveAuthenticationResponse
{
    [JsonPropertyName("completed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Completed { get; init; }

    [JsonPropertyName("flows")]
    public required IReadOnlyList<FlowInformation> Flows { get; init; }

    [JsonPropertyName("params")]
    public required IReadOnlyDictionary<string, object> Params { get; init; }

    [JsonPropertyName("session")]
    public required string Session { get; init; }

    [JsonPropertyName("errcode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; init; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; init; }

    public sealed class FlowInformation
    {
        [JsonPropertyName("stages")]
        public required IReadOnlyList<string> Stages { get; init; }
    }
}