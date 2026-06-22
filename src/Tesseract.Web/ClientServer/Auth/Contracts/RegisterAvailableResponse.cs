using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class RegisterAvailableResponse
{
    [JsonPropertyName("available")]
    public required bool Available { get; init; }
}