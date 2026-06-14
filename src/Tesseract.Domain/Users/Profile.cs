namespace Tesseract.Domain.Users;

public sealed class Profile(UserId userId, string? displayName, string? avatarUrl)
{
    public UserId UserId { get; } = userId;
    public string? DisplayName { get; } = displayName;
    public string? AvatarUrl { get; } = avatarUrl;
}