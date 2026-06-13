using Microsoft.Extensions.Options;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Domain.Discovery.Values;

namespace Tesseract.Infrastructure.ClientServer.Discovery;

public class WellKnownRepository(IOptions<MatrixOptions> options) : IWellKnownRepository
{
    public Task<DiscoveryInfo?> GetDiscoveryInfo(CancellationToken cancellationToken)
    {
        var matrix = options.Value;

        if (matrix.Homeserver is null && matrix.IdentityServer is null)
        {
            return Task.FromResult<DiscoveryInfo?>(null);
        }

        if (matrix.Homeserver is null || string.IsNullOrWhiteSpace(matrix.Homeserver.BaseUrl))
        {
            throw new InvalidOperationException(
                "Matrix:Homeserver:BaseUrl configuration is required when Matrix section is present.");
        }

        string? identityServerBaseUrl = null;
        if (matrix.IdentityServer is not null && !string.IsNullOrWhiteSpace(matrix.IdentityServer.BaseUrl))
        {
            identityServerBaseUrl = matrix.IdentityServer.BaseUrl;
        }

        return Task.FromResult<DiscoveryInfo?>(new DiscoveryInfo(
            matrix.Homeserver.BaseUrl,
            identityServerBaseUrl));
    }
}