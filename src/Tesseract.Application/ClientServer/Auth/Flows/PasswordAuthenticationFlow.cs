using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.ClientServer.Auth.Flows;

public class PasswordAuthenticationFlow(
    IPasswordHasher passwordHasher,
    IUserRepository userRepository,
    IPasswordRepository passwordRepository,
    IMatrixConfigurationRepository matrixConfigurationRepository)
    : IAuthenticationFlow
{
    public string Type => "m.login.password";

    public async Task<User?> AuthenticateAsync(string? login, string? passwordRaw, CancellationToken cancellationToken)
    {
        // FIXME: This implementation is vulnerable to timing attacks.
        if (await GetUserHandleAsync(login, cancellationToken) is not { } handle
            || await userRepository.GetByHandleAsync(handle, cancellationToken) is not { } user
            || await passwordRepository.GetHashAsync(user.Id, cancellationToken) is not { } password
            || !await passwordHasher.VerifyAsync(passwordRaw ?? string.Empty, password.Hash, cancellationToken))
        {
            return null;
        }

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