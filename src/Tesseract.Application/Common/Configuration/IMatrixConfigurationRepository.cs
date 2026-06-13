namespace Tesseract.Application.Common.Configuration;

public interface IMatrixConfigurationRepository
{
    Task<Domain.Users.Values.Domain> GetDomainAsync(CancellationToken cancellationToken);
}