using MediatR;

namespace Tesseract.Domain;

public abstract class AggregateRoot<TIdentifier, TNotification>(TIdentifier id) where TNotification : INotification
{
    private readonly List<TNotification> _events = [];

    public TIdentifier Id { get; } = id;

    public IReadOnlyCollection<TNotification> Events => _events.AsReadOnly();

    protected void RaiseEvent(TNotification @event) => _events.Add(@event);

    public void ClearEvents() => _events.Clear();
}