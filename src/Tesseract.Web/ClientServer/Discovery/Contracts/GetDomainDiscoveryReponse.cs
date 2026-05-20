using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Discovery.Contracts;

public sealed class GetDomainDiscoveryResponse
{
    [JsonPropertyName("m.homeserver")]
    public required HomeserverInfoResponse Homeserver { get; init; }

    [JsonPropertyName("m.identity_server")]
    public IdentityServerInfoResponse? IdentityServer { get; set; }
}