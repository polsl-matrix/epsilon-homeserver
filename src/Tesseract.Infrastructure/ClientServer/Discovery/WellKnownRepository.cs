using Microsoft.Extensions.Configuration;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Discovery.Abstractions;

namespace Tesseract.Infrastructure.ClientServer.Discovery;

public class WellKnownRepository(IConfiguration configuration) : IWellKnownRepository
{
    public Task<DiscoveryInfo?> GetDiscoveryInfo(CancellationToken cancellationToken)
    {
        var homeserverBaseUrl = configuration["Matrix:Homeserver:BaseUrl"];

        if (string.IsNullOrWhiteSpace(homeserverBaseUrl))
        {
            return Task.FromResult<DiscoveryInfo?>(null);
        }

        var identityServerBaseUrl = configuration["Matrix:IdentityServer:BaseUrl"];

        return Task.FromResult<DiscoveryInfo?>(new DiscoveryInfo(
            homeserverBaseUrl,
            string.IsNullOrWhiteSpace(identityServerBaseUrl) ? null : identityServerBaseUrl));
    }
}