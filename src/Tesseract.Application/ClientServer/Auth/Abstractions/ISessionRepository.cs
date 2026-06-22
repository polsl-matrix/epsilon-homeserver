using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface ISessionRepository
{
    Task<Session?> GetByAccessTokenAsync(byte[] accessTokenHash, CancellationToken cancellationToken);
    Task UpsertAsync(Session session, CancellationToken cancellationToken);
    Task DeleteAllByUserIdAsync(UserId userId, CancellationToken cancellationToken);
}