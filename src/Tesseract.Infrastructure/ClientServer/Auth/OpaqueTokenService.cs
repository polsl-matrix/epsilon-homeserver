using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Tesseract.Application.ClientServer.Auth;

namespace Tesseract.Infrastructure.ClientServer.Auth;

public class OpaqueTokenService : IAccessTokenService, IRefreshTokenService
{
    public const int RawByteCount = 32;

    public Task<string> Create(CancellationToken _)
    {
        var token = RandomNumberGenerator.GetBytes(RawByteCount);

        var encoded = Base64UrlEncoder.Encode(token);
        return Task.FromResult(encoded);
    }
}