using Tesseract.Domain;

namespace Tesseract.Application.ClientServer.Rooms;

public interface IEventRepository
{
    Task InsertAsync(Event @event, CancellationToken cancellationToken);
}