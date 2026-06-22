namespace Tesseract.Domain.Rooms;

public readonly record struct RoomMessageId(Guid Value)
{
    public static RoomMessageId Random() => new(Guid.NewGuid());
}