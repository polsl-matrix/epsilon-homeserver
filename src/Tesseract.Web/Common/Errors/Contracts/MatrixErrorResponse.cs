using System.Text.Json.Serialization;

namespace Tesseract.Web.Common.Errors.Contracts;

public record MatrixErrorResponse(
    [property: JsonPropertyName("errcode")]
    string Code,
    [property: JsonPropertyName("error")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Message = null
);