using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Discovery.Contracts;

// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class GetSupportedVersionsResponse
{
    [JsonPropertyName("versions")]
    public required IReadOnlyCollection<string> Versions { get; init; }
}
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore NotAccessedField.Global