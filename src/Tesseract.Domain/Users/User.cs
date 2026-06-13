using Tesseract.Domain.Users.Values;

namespace Tesseract.Domain.Users;

public sealed class User(UserId id, UserHandle handle)
{
    public UserId Id { get; } = id;
    public UserHandle Handle { get; } = handle;
}