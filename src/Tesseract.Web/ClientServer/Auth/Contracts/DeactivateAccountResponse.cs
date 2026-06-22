using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class DeactivateAccountResponse
{
    [JsonPropertyName("id_server_unbind_result")]
    public required string IdServerUnbindResult { get; init; }
}