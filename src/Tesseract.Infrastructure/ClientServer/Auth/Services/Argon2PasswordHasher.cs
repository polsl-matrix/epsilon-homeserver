using Isopoh.Cryptography.Argon2;
using System.Text;
using Tesseract.Application.ClientServer.Auth.Abstractions;

namespace Tesseract.Infrastructure.ClientServer.Auth.Services;

public class Argon2PasswordHasher : IPasswordHasher
{
    public Task<byte[]> HashAsync(string input, CancellationToken cancellationToken)
    {
        var hash = Argon2.Hash(input);
        var bytes = Encoding.UTF8.GetBytes(hash);

        return Task.FromResult(bytes);
    }

    public Task<bool> VerifyAsync(string input, byte[] hash, CancellationToken cancellationToken)
    {
        var encoded = Encoding.UTF8.GetString(hash);
        var success = Argon2.Verify(encoded, input);

        return Task.FromResult(success);
    }
}