using Tesseract.Domain.Rooms;

namespace Tesseract.Application.ClientServer.Rooms.Abstractions;

public interface IRoomRepository
{
    Task SaveAsync(Room room, CancellationToken cancellationToken);
}