namespace Tesseract.Application.ClientServer.Auth;

public interface ITokenService
{
    Task<string> Create(CancellationToken cancellationToken);
}