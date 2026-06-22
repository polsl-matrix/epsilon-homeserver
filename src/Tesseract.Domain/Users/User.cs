namespace Tesseract.Domain.Users;

public sealed class User(UserId id, UserHandle handle, bool deactivated = false)
{
    public UserId Id { get; } = id;
    public UserHandle Handle { get; } = handle;
    public bool Deactivated { get; } = deactivated;
}