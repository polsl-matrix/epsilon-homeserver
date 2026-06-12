using Tesseract.Domain.Users.Values;

namespace Tesseract.Domain.Users;

public sealed class User(UserId id, Handle handle)
{
    public UserId Id { get; } = id;
    public Handle Handle { get; } = handle;
}