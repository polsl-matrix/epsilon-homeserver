using System.Buffers.Text;
using System.Security.Cryptography;
using Tesseract.Application.ClientServer.Registration.Abstractions;

namespace Tesseract.Infrastructure.Common.Security;

public sealed class SecureIdentifierGenerator : IIdentifierGenerator
{
    private const string DeviceIdAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int DeviceIdLength = 10;
    private const int AccessTokenByteLength = 32;

    public string GenerateLocalpart() =>
        Guid.CreateVersion7().ToString("N");

    public string GenerateDeviceId() =>
        RandomNumberGenerator.GetString(DeviceIdAlphabet, DeviceIdLength);

    public string GenerateAccessToken() =>
        Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(AccessTokenByteLength));
}