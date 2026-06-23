namespace Tesseract.Infrastructure.ClientServer.Identity.Dao;

internal sealed class ProfileDao
{
    public required Guid UserId { get; init; }
    public required string? DisplayName { get; init; }
    public required string? AvatarUrl { get; init; }
}