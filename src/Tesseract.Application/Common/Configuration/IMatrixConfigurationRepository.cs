namespace Tesseract.Application.Common.Configuration;

public interface IMatrixConfigurationRepository
{
    Task<Domain.Common.Values.Domain> GetDomainAsync(CancellationToken cancellationToken);
}