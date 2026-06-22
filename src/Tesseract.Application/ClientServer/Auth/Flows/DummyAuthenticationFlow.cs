using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Flows;

internal class DummyAuthenticationFlow : IAuthenticationFlow
{
    public string Type => "m.login.dummy";

    public Task<User?> AuthenticateAsync(string? login, string? password, CancellationToken cancellationToken)
    {
        var handle = new UserHandle("dummy", "domain");
        var user = new User(new UserId(Guid.Empty), handle);

        return Task.FromResult<User?>(user);
    }
}