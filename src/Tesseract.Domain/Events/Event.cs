using MediatR;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Events;

public abstract class Event(string type, Room room, User sender, string? stateKey, VDomain domain)
    : INotification
{
    public EventId Id { get; } = EventId.Random();
    public EventHandle Handle { get; } = EventHandle.Random(domain.Value);
    public string Type { get; } = type;
    public Room Room { get; } = room;
    public User Sender { get; } = sender;
    public DateTime Timestamp { get; } = DateTime.Now;
    public string? StateKey { get; } = stateKey;
}