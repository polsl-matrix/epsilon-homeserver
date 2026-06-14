namespace Tesseract.Infrastructure.ClientServer.Auth.Dao;

internal sealed class SessionDao
{
    public required Guid SessionId { get; init; }
    public required Guid UserId { get; init; }
    public required byte[] CurrentAccessTokenHash { get; init; }
    public required byte[] CurrentRefreshTokenHash { get; init; }
    public byte[]? PendingAccessTokenHash { get; init; }
    public byte[]? PendingRefreshTokenHash { get; init; }
}