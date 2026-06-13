namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IPasswordHasher : IHashService
{
    Task<bool> VerifyAsync(string input, byte[] hash, CancellationToken cancellationToken);
}