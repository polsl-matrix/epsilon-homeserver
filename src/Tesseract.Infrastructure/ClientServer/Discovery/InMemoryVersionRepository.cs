using Tesseract.Application.ClientServer.Discovery.Abstractions;

namespace Tesseract.Infrastructure.ClientServer.Discovery;

public class InMemoryVersionRepository : IVersionRepository
{
    private static readonly string[] SupportedVersions =
    [
        "v1.11",
    ];

    public Task<IEnumerable<string>> GetSupportedVersionsAsync(CancellationToken cancellationToken) =>
        Task.FromResult(SupportedVersions.AsEnumerable());
}