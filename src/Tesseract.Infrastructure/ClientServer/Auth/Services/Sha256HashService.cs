using System.Security.Cryptography;
using System.Text;
using Tesseract.Application.ClientServer.Auth.Abstractions;

namespace Tesseract.Infrastructure.ClientServer.Auth.Services;

internal class Sha256HashService : IHashService
{
    public Task<byte[]> HashAsync(string input, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);

        return Task.FromResult(hash);
    }
}