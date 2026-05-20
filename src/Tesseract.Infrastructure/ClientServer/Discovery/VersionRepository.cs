using Tesseract.Application.ClientServer.Discovery.Abstractions;

namespace Tesseract.Infrastructure.ClientServer.Discovery;

public class VersionRepository : IVersionRepository
{
    private static readonly string[] SupportedVersions =
    [
        "v1.11",
    ];

    public Task<IEnumerable<string>> GetSupportedVersions(CancellationToken cancellationToken) =>
        Task.FromResult(SupportedVersions.AsEnumerable());
}