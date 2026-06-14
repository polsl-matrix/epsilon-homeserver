using System.Security.Cryptography;
using Tesseract.Application.ClientServer.Auth.Abstractions;

namespace Tesseract.Infrastructure.ClientServer.Auth.Services;

internal sealed class RandomDeviceIdGenerator : IDeviceIdGenerator
{
    public string Create() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
}