using Tesseract.Domain.Events;
using Tesseract.Infrastructure.ClientServer.Events.Dao;

namespace Tesseract.Infrastructure.ClientServer.Events;

internal abstract class EventMapper<TDomain, TDao> : IEventMapper
    where TDomain : Event where TDao : EventDao
{
    public abstract string Type { get; }

    public EventDao ToDao(Event @event)
    {
        if (@event is not TDomain typedEvent)
        {
            throw new ArgumentException("Invlid event type.");
        }

        return ToDao(typedEvent);
    }

    protected abstract TDao ToDao(TDomain typedEvent);
}