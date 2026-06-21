namespace Tesseract.Domain.Rooms;

public interface IRoomMembershipRepository
{
    Task InsertAsync(RoomMembership roomMembership, CancellationToken cancellationToken);
}