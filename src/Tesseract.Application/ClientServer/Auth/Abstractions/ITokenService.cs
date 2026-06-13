namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface ITokenService
{
    Task<string> CreateAsync(CancellationToken cancellationToken);
}

public interface IAccessTokenService : ITokenService;

public interface IRefreshTokenService : ITokenService;