using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Models;

public sealed record Session(
    SessionId Id,
    UserId UserId,
    byte[] AccessTokenHash,
    byte[] RefreshTokenHash,
    byte[]? PendingAccessTokenHash,
    byte[]? PendingRefreshTokenHash)
{
    public Session(SessionId id, UserId userId, byte[] accessTokenHash, byte[] refreshTokenHash)
        : this(id, userId, accessTokenHash, refreshTokenHash, null, null) { }
}