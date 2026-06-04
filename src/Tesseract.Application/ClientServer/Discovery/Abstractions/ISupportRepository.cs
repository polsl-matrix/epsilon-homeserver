using Tesseract.Domain.Support;

namespace Tesseract.Application.ClientServer.Discovery.Abstractions;

public interface ISupportRepository
{
    Task<SupportInfo> GetSupportInfo(CancellationToken cancellationToken);
}