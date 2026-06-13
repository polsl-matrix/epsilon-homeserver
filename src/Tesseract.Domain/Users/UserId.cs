namespace Tesseract.Domain.Users;

public readonly record struct UserId(Guid Value)
{
    public static UserId Random() => new(Guid.NewGuid());
}