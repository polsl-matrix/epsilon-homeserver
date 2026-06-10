using System.Text.Json.Serialization;

namespace Tesseract.Web.Common.Errors.Contracts;

// @formatter:off
public record MatrixErrorResponse(
    [property: JsonPropertyName("errcode")] string Code,
    [property: JsonPropertyName("error")] string? Message = null);
// @formatter:on