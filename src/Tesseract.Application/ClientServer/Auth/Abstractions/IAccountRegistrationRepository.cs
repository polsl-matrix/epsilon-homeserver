using Tesseract.Application.ClientServer.Auth.Models;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IAccountRegistrationRepository
{
    Task CreateAsync(AccountRegistration registration, CancellationToken cancellationToken);
}