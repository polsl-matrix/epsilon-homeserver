using Tesseract.Domain.Users;

namespace Tesseract.Domain.Rooms;

public interface IRoomMembershipRepository
{
    Task InsertAsync(RoomMembership roomMembership, CancellationToken cancellationToken);
    Task<RoomMembership?> GetByRoomIdAndUserIdAsync(RoomId roomId, UserId userId, CancellationToken cancellationToken);
}