namespace Tesseract.Domain.Rooms;

public interface IRoomMembershipRepository
{
    Task SaveAsync(RoomMembership roomMembership, CancellationToken cancellationToken);
}