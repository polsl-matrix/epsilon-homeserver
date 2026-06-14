using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Models;

public sealed class Session(
    SessionId id,
    UserId userId,
    byte[] currentAccessTokenHash,
    byte[] currentRefreshTokenHash,
    byte[]? pendingAccessTokenHash = null,
    byte[]? pendingRefreshTokenHash = null)
{
    public SessionId Id { get; } = id;
    public UserId UserId { get; } = userId;
    public byte[] CurrentAccessTokenHash { get; private set; } = currentAccessTokenHash;
    public byte[] CurrentRefreshTokenHash { get; private set; } = currentRefreshTokenHash;
    public byte[]? PendingAccessTokenHash { get; private set; } = pendingAccessTokenHash;
    public byte[]? PendingRefreshTokenHash { get; private set; } = pendingRefreshTokenHash;

    public bool HasPendingAccessTokenHash(byte[] tokenHash) =>
        PendingAccessTokenHash is { } pendingHash && pendingHash.SequenceEqual(tokenHash);

    public bool HasPendingRefreshTokenHash(byte[] tokenHash) =>
        PendingRefreshTokenHash is { } pendingHash && pendingHash.SequenceEqual(tokenHash);

    public void PromotePendingTokens()
    {
        if (PendingAccessTokenHash is not { } pendingAccessTokenHashValue ||
            PendingRefreshTokenHash is not { } pendingRefreshTokenHashValue)
        {
            throw new InvalidOperationException("Both pending token hashes are required for promotion.");
        }

        CurrentAccessTokenHash = pendingAccessTokenHashValue;
        CurrentRefreshTokenHash = pendingRefreshTokenHashValue;
        PendingAccessTokenHash = null;
        PendingRefreshTokenHash = null;
    }

    public void SetPendingTokens(byte[] accessTokenHash, byte[] refreshTokenHash)
    {
        PendingAccessTokenHash = accessTokenHash;
        PendingRefreshTokenHash = refreshTokenHash;
    }
}