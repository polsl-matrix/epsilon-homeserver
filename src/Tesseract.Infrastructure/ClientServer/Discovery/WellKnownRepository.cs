using Microsoft.Extensions.Options;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Domain.Discovery.Values;
using Tesseract.Infrastructure.ClientServer.Discovery.Configuration;

namespace Tesseract.Infrastructure.ClientServer.Discovery;

public class WellKnownRepository(IOptions<DiscoveryOptions> options) : IWellKnownRepository
{
    public Task<DiscoveryInfo?> GetDiscoveryInfo(CancellationToken cancellationToken)
    {
        var value = options.Value;

        var homeserverBaseUrl = value.HomeserverBaseUrl;
        var identityServerBaseUrl = value.IdentityServerBaseUrl;

        return Task.FromResult<DiscoveryInfo?>(
            new DiscoveryInfo(homeserverBaseUrl, identityServerBaseUrl));
    }
}