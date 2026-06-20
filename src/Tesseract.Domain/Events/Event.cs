using MediatR;
using Tesseract.Domain.Events;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain;

public class Event : INotification
{
    public EventId Id { get; } = EventId.Random();
    public required string Type { get; init; }
    public required RoomId RoomId { get; init; }
    public required UserId SenderId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;

    public string? StateKey { get; init; }
}