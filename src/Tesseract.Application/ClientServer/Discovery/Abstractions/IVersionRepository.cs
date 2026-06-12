namespace Tesseract.Application.ClientServer.Discovery.Abstractions;

public interface IVersionRepository
{
    Task<IEnumerable<string>> GetSupportedVersionsAsync(CancellationToken cancellationToken);
}