using Tesseract.Domain.Rooms;

namespace Tesseract.Application.ClientServer.Rooms;

public interface IRoomRepository
{
    Task SaveAsync(Room room, CancellationToken cancellationToken);
}