using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IAuthenticationFlow
{
    string Type { get; }

    Task<User?> AuthenticateAsync(string? login, string? password,
        CancellationToken cancellationToken);
}