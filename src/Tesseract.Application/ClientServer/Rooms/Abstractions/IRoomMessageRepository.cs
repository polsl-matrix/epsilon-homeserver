using Tesseract.Domain.Rooms;

namespace Tesseract.Application.ClientServer.Rooms.Abstractions;

public interface IRoomMessageRepository
{
    Task InsertAsync(RoomMessage message, CancellationToken cancellationToken);
}