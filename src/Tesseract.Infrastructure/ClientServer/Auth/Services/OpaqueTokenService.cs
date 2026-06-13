using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Tesseract.Application.ClientServer.Auth.Abstractions;

namespace Tesseract.Infrastructure.ClientServer.Auth.Services;

internal class OpaqueTokenService : IAccessTokenService, IRefreshTokenService
{
    public const int RawByteCount = 32;

    public Task<string> CreateAsync(CancellationToken _)
    {
        var token = RandomNumberGenerator.GetBytes(RawByteCount);

        var encoded = Base64UrlEncoder.Encode(token);
        return Task.FromResult(encoded);
    }
}