namespace Tesseract.Application.ClientServer.Auth;

public interface ITokenService
{
    Task<string> Create(CancellationToken cancellationToken);
}

public interface IAccessTokenService : ITokenService;

public interface IRefreshTokenService : ITokenService;