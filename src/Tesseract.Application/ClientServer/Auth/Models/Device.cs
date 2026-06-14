using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Models;

public sealed class Device(UserId userId, string id, string? displayName)
{
    public UserId UserId { get; } = userId;
    public string Id { get; } = id;
    public string? DisplayName { get; } = displayName;
}