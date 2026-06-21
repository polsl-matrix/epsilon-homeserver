using MediatR;
using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class Event(EventId id, string type, Room room, User sender, DateTime timestamp, string? stateKey)
    : INotification
{
    [SetsRequiredMembers]
    protected Event(string type, Room room, User sender, string? stateKey)
        : this(EventId.Random(), type, room, sender, DateTime.Now, stateKey)
    {
    }

    public EventId Id { get; } = id;
    public required string Type { get; init; } = type;
    public required Room Room { get; init; } = room;
    public required User Sender { get; init; } = sender;
    public DateTime Timestamp { get; init; } = timestamp;

    public string? StateKey { get; init; } = stateKey;
}