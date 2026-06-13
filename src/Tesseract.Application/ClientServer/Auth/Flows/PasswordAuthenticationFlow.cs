using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.ClientServer.Auth.Flows;

public class PasswordAuthenticationFlow(
    IUserRepository userRepository,
    IMatrixConfigurationRepository matrixConfigurationRepository)
    : IAuthenticationFlow
{
    public string Type => "m.login.password";

    public async Task<User?> AuthenticateAsync(string? login, string? password, CancellationToken cancellationToken)
    {
        if (await GetUserHandleAsync(login, cancellationToken) is not { } handle)
        {
            return null;
        }

        var user = await userRepository.GetByHandleAsync(handle, cancellationToken);

        // TODO: Check password hash.
        // TODO: Return user if correct.

        return user;
    }

    private async Task<UserHandle?> GetUserHandleAsync(string? login, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(login))
        {
            return null;
        }

        var domain = await matrixConfigurationRepository.GetDomainAsync(cancellationToken);

        if (Localpart.TryParse(login, out var localpart))
        {
            return new UserHandle(localpart.Value, domain.Value);
        }

        if (UserHandle.TryParse(login, out var handle) && handle.Domain != domain)
        {
            return handle;
        }

        return null;
    }
}