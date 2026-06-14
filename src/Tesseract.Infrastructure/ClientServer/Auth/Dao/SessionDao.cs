namespace Tesseract.Infrastructure.ClientServer.Auth.Dao;

internal sealed class SessionDao
{
    public required Guid SessionId { get; init; }
    public required Guid UserId { get; init; }
    public string? DeviceId { get; init; }
    public required byte[] CurrentAccessTokenHash { get; init; }
    public required byte[] CurrentRefreshTokenHash { get; init; }
}