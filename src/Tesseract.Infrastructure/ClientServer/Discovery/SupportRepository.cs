using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Domain.Support;
using Tesseract.Domain.Support.Abstractions;
using Tesseract.Infrastructure.Configuration;

namespace Tesseract.Infrastructure.ClientServer.Discovery;

public class SupportRepository(IOptions<SupportInfoOptions> supportInfoOptions) : ISupportRepository
{
    private readonly SupportInfoOptions _supportInfoOptions = supportInfoOptions.Value;

    public Task<SupportInfo> GetSupportInfo(CancellationToken cancellationToken)
    {
        var contacts = _supportInfoOptions.Contacts.Select(c => 
            new Contact(c.EmailAddress, c.MatrixId, c.Role)
        ).Cast<IContact>().ToList();
        var supportInfo = new SupportInfo(contacts, _supportInfoOptions.SupportPage);
        return Task.FromResult(supportInfo);
    }
    
}