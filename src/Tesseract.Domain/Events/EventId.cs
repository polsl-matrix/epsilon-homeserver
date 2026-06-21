namespace Tesseract.Domain.Events;

public readonly record struct EventId(Guid Value)
{
    public static EventId Random() => new(Guid.NewGuid());
}