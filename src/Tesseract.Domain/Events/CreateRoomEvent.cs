using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class CreateRoomEvent(Room roomId, User creator)
    : Event(EventTypes.CreateRoom, roomId, creator, string.Empty);