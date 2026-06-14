using System.Security.Cryptography;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Infrastructure.ClientServer.Auth.Services;

internal sealed class RandomLocalpartGenerator : ILocalpartGenerator
{
    public Localpart Create() => new($"u{Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant()}");
}