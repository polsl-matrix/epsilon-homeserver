using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Models;

public sealed record Session(
    SessionId Id,
    UserId UserId,
    byte[] AccessTokenHash,
    byte[] RefreshTokenHash,
    string? DeviceId = null)
{
}