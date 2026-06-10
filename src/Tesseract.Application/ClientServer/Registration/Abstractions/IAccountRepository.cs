using Tesseract.Domain.Accounts.Values;

namespace Tesseract.Application.ClientServer.Registration.Abstractions;

public interface IAccountRepository
{
    Task<bool> IsLocalpartTaken(string localpart, CancellationToken cancellationToken);

    Task CreateAccount(Account account, Device? device, CancellationToken cancellationToken);
}