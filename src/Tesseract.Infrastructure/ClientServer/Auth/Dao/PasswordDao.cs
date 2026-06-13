namespace Tesseract.Infrastructure.ClientServer.Auth.Dao;

internal sealed class PasswordDao
{
    public required Guid UserId { get; init; }
    public required byte[] PasswordHash { get; init; }
}