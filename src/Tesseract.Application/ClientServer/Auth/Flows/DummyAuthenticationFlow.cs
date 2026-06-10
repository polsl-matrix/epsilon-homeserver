using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users.Entities;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.ClientServer.Auth.Flows;

public class DummyAuthenticationFlow : IAuthenticationFlow
{
    public string Type => "m.login.dummy";

    public Task<User?> AuthenticateAsync(string login, string password, CancellationToken cancellationToken)
    {
        var handle = new Handle("dummy", "domain");
        var user = new User(Guid.Empty, handle);

        return Task.FromResult<User?>(user);
    }
}