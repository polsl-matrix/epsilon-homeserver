using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Models;

public sealed class AccountRegistration(
    User user,
    Profile profile,
    byte[] passwordHash,
    Device? device,
    Session? session)
{
    public User User { get; } = user;
    public Profile Profile { get; } = profile;
    public byte[] PasswordHash { get; } = passwordHash;
    public Device? Device { get; } = device;
    public Session? Session { get; } = session;
}