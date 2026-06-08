using Tesseract.Domain.Discovery.Values;

namespace Tesseract.Application.ClientServer.Discovery.Abstractions;

public interface IWellKnownRepository
{
    Task<DiscoveryInfo?> GetDiscoveryInfo(CancellationToken cancellationToken);
}