using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Registration.Contracts;

public sealed class UserInteractiveAuthResponse
{
    [JsonPropertyName("flows")]
    public required IReadOnlyCollection<FlowInformation> Flows { get; init; }

    [JsonPropertyName("session")]
    public required string Session { get; init; }

    public sealed class FlowInformation
    {
        [JsonPropertyName("stages")]
        public required IReadOnlyCollection<string> Stages { get; init; }
    }
}