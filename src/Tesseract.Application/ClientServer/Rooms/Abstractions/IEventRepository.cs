using Tesseract.Domain.Events;

namespace Tesseract.Application.ClientServer.Rooms.Abstractions;

public interface IEventRepository
{
    Task InsertAsync(Event @event, CancellationToken cancellationToken);
}