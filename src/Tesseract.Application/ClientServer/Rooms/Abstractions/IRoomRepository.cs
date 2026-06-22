using Tesseract.Domain.Rooms;

namespace Tesseract.Application.ClientServer.Rooms.Abstractions;

public interface IRoomRepository
{
    Task InsertAsync(Room room, CancellationToken cancellationToken);
    Task<Room?> GetByHandleAsync(RoomHandle handle, CancellationToken cancellationToken);
}