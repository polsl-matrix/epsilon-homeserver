namespace Tesseract.Application.ClientServer.Discovery.Abstractions;

public interface IVersionRepository
{
    Task<IEnumerable<string>> GetSupportedVersions(CancellationToken cancellationToken);
}