using Tesseract.Domain.Rooms;

namespace Tesseract.Application.ClientServer.Rooms.Abstractions;

public interface IRoomRepository
{
    Task InsertAsync(Room room, CancellationToken cancellationToken);
}