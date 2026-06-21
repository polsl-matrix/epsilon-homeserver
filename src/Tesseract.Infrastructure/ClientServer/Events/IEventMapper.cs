using Tesseract.Domain.Events;
using Tesseract.Infrastructure.ClientServer.Events.Dao;

namespace Tesseract.Infrastructure.ClientServer.Events;

internal interface IEventMapper
{
    string Type { get; }

    EventDao ToDao(Event @event);
}