using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Events;

[method: SetsRequiredMembers]
public class CreateRoomEvent(RoomId roomId, UserId creatorId)
    : Event(EventTypes.CreateRoom, roomId, creatorId, string.Empty);