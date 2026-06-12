using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.ClientServer.Auth.Flows;

public class DummyAuthenticationFlow : IAuthenticationFlow
{
    public string Type => "m.login.dummy";

    public Task<User?> AuthenticateAsync(string? login, string? password, CancellationToken cancellationToken)
    {
        var handle = new Handle("dummy", "domain");
        var user = new User(new UserId(Guid.Empty), handle);

        return Task.FromResult<User?>(user);
    }
}