using System.Text.Json.Serialization;

namespace Tesseract.Web.ClientServer.Discovery.Contracts;

public sealed class GetDomainDiscoveryResponse
{
    [JsonPropertyName("m.homeserver")]
    public required HomeserverInfo Homeserver { get; init; }

    [JsonPropertyName("m.identity_server")]
    public IdentityServerInfo? IdentityServer { get; set; }

    public sealed class HomeserverInfo
    {
        [JsonPropertyName("base_url")]
        public required string BaseUrl { get; init; }
    }

    public sealed class IdentityServerInfo
    {
        [JsonPropertyName("base_url")]
        public required string BaseUrl { get; init; }
    }
}