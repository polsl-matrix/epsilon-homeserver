namespace Tesseract.Domain.Rooms;

public readonly record struct RoomId(Guid Value)
{
    public static RoomId Random() => new(Guid.NewGuid());
}