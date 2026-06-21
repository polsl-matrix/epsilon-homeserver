using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class CreateRoomEvent(Room room, User creator)
    : Event(EventTypes.CreateRoom, room, creator, string.Empty);