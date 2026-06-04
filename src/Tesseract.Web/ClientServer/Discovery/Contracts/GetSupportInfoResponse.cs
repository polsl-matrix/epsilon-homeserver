using System.Text.Json.Serialization;
using Tesseract.Domain.Support;
using Tesseract.Domain.Support.Abstractions;

namespace Tesseract.Web.ClientServer.Discovery.Contracts;

public sealed class GetSupportInfoResponse
{
    [JsonPropertyName("contacts")]
    public required IReadOnlyList<IContact> Contacts { get; init; }
    [JsonPropertyName("support_page")]
    public required string SupportPage { get; init; }
}