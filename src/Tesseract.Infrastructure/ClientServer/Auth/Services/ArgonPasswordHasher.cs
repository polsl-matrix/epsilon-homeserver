using Isopoh.Cryptography.Argon2;
using System.Text;
using Tesseract.Application.ClientServer.Auth.Abstractions;

namespace Tesseract.Infrastructure.ClientServer.Auth.Services;

public class ArgonPasswordHasher : IPasswordHasher
{
    public Task<byte[]> HashAsync(string input, CancellationToken cancellationToken)
    {
        var hash = Argon2.Hash(input);
        var bytes = Encoding.UTF8.GetBytes(hash);

        return Task.FromResult(bytes);
    }

    public Task<bool> VerifyAsync(string input, byte[] hash, CancellationToken cancellationToken)
    {
        var encoded = hash.ToString();
        var success = Argon2.Verify(encoded, input);

        return Task.FromResult(success);
    }
}