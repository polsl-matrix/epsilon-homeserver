using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

internal sealed class SessionFactory(
    IHashService hashService,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService)
    : ISessionFactory
{
    public async Task<(Session, string AccessToken, string RefreshToken)> CreateAsync(
        User user, CancellationToken cancellationToken)
    {
        var (accessTokenRaw, accessTokenHash) = await CreateTokenAsync(accessTokenService, cancellationToken);
        var (refreshTokenRaw, refreshTokenHash) = await CreateTokenAsync(refreshTokenService, cancellationToken);

        var session = new Session(SessionId.Random(), user.Id, accessTokenHash, refreshTokenHash);

        return new ValueTuple<Session, string, string>(session, accessTokenRaw, refreshTokenRaw);
    }

    private async Task<(string, byte[])> CreateTokenAsync(
        ITokenService tokenService, CancellationToken cancellationToken)
    {
        var token = await tokenService.Create(cancellationToken);
        var hash = await hashService.HashAsync(token, cancellationToken);
        return (token, hash);
    }
}