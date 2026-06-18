namespace Tesseract.Domain;

// TODO: Move it to the right folder.
public abstract class AggregateRoot<TIdentifier>(TIdentifier id)
{
    private readonly List<DomainEvent> _events = [];

    public TIdentifier Id { get; } = id;

    public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

    protected void RaiseEvent(DomainEvent domainEvent) => _events.Add(domainEvent);

    public void ClearEvents() => _events.Clear();
}