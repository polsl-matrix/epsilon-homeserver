namespace Tesseract.Infrastructure.ClientServer.Rooms.Dao;

internal sealed class RoomMembershipDao
{
    public required Guid RoomId { get; init; }
    public required Guid UserId { get; init; }
}