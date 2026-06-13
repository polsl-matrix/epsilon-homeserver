using Microsoft.Extensions.Options;
using Tesseract.Application.Common.Configuration;
using VDomain = Tesseract.Domain.Users.Values.Domain;

namespace Tesseract.Infrastructure.Common.Configuration;

public class MatrixConfigurationRepository(IOptions<MatrixConfigurationOptions> options)
    : IMatrixConfigurationRepository
{
    public Task<VDomain> GetDomainAsync(CancellationToken _)
    {
        var value = options.Value.Domain;
        var domain = new VDomain(value);

        return Task.FromResult(domain);
    }
}