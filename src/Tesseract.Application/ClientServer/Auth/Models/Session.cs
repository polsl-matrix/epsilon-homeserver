using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Models;

public sealed class Session(SessionId id, UserId userId, byte[] accessTokenHash, byte[] refreshTokenHash)
{
    public SessionId Id { get; } = id;
    public UserId UserId { get; } = userId;
    public byte[] AccessTokenHash { get; set; } = accessTokenHash;
    public byte[] RefreshTokenHash { get; set; } = refreshTokenHash;
}