using Tesseract.Domain.Users.Values;

namespace Tesseract.Domain.Users;

public sealed class User(Guid id, Handle handle)
{
    public Guid Id { get; } = id;
    public Handle Handle { get; } = handle;
}