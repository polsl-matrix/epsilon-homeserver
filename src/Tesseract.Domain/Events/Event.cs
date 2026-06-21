using MediatR;
using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Events;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain;

[method: SetsRequiredMembers]
public class Event(EventId id, string type, RoomId roomId, UserId senderId, DateTime timestamp, string? stateKey)
    : INotification
{
    [SetsRequiredMembers]
    protected Event(string type, RoomId roomId, UserId senderId, string? stateKey)
        : this(EventId.Random(), type, roomId, senderId, DateTime.Now, stateKey)
    {
    }

    public EventId Id { get; } = id;
    public required string Type { get; init; } = type;
    public required RoomId RoomId { get; init; } = roomId;
    public required UserId SenderId { get; init; } = senderId;
    public DateTime Timestamp { get; init; } = timestamp;

    public string? StateKey { get; init; } = stateKey;
}