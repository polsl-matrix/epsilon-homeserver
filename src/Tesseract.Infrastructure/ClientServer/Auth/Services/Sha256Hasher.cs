using System.Security.Cryptography;
using System.Text;
using Tesseract.Application.ClientServer.Auth.Abstractions;

namespace Tesseract.Infrastructure.ClientServer.Auth.Services;

internal class Sha256Hasher : IHashService
{
    public Task<byte[]> HashAsync(string input, CancellationToken cancellationToken)
    {
        var hash = Hash(input);
        return Task.FromResult(hash);
    }

    private static byte[] Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return SHA256.HashData(bytes);
    }
}