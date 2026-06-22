using Tesseract.Domain.Events;
using Tesseract.Domain.Rooms;

namespace Tesseract.Application.ClientServer.Rooms.Abstractions;

public interface IEventRepository
{
    Task InsertAsync(Event @event, CancellationToken cancellationToken);
    Task<IEnumerable<string>> GetByRoomIdAsync(RoomId roomId, CancellationToken cancellationToken);
    Task<IEnumerable<string>> GetByTypeAndRoomIdAsync(string type, RoomId roomId, CancellationToken cancellationToken);
}