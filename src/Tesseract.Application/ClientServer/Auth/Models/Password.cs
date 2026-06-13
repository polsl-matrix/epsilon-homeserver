using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Models;

public sealed class Password(UserId userId, byte[] hash)
{
    public UserId UserId { get; } = userId;
    public byte[] Hash { get; } = hash;
}