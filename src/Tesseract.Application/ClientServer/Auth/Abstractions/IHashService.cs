namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IHashService
{
    Task<byte[]> HashAsync(string input, CancellationToken cancellationToken);
}