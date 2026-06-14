using Tesseract.Application.ClientServer.Auth.Models;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface ISessionRepository
{
    Task<Session?> GetByAccessTokenAsync(byte[] accessTokenHash, CancellationToken cancellationToken);
    Task<Session?> GetByRefreshTokenAsync(byte[] refreshTokenHash, CancellationToken cancellationToken);
    Task<bool> PromotePendingTokensByAccessTokenAsync(byte[] accessTokenHash, CancellationToken cancellationToken);
    Task<bool> RotateRefreshTokenAsync(
        byte[] refreshTokenHash,
        byte[] pendingAccessTokenHash,
        byte[] pendingRefreshTokenHash,
        CancellationToken cancellationToken);
    Task UpsertAsync(Session session, CancellationToken cancellationToken);
}