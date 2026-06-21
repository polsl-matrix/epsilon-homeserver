using Tesseract.Domain;
using Tesseract.Infrastructure.ClientServer.Events.Dao;

namespace Tesseract.Infrastructure.ClientServer.Events;

public interface IEventMapper
{
    string Type { get; }

    EventDao ToDao(Event @event);
}