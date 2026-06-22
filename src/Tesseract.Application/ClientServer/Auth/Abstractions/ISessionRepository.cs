using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(SessionId sessionId, CancellationToken cancellationToken);
    Task<Session?> GetByAccessTokenAsync(byte[] accessTokenHash, CancellationToken cancellationToken);
    Task UpsertAsync(Session session, CancellationToken cancellationToken);
    Task DeleteByIdAsync(SessionId sessionId, CancellationToken cancellationToken);
    Task DeleteByUserIdAsync(UserId userId, CancellationToken cancellationToken);
}