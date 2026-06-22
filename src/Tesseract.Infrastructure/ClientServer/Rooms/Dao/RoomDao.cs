namespace Tesseract.Infrastructure.ClientServer.Rooms.Dao;

internal sealed class RoomDao
{
    public required Guid RoomId { get; init; }
    public required string Localpart { get; init; }
    public required string Domain { get; init; }
}