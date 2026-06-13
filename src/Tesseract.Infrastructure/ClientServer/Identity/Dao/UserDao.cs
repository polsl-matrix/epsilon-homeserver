namespace Tesseract.Infrastructure.ClientServer.Identity.Dao;

internal sealed class UserDao
{
    public required Guid UserId { get; init; }
    public required string Localpart { get; init; }
    public required string Domain { get; init; }
}