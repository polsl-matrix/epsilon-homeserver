using System.Diagnostics.CodeAnalysis;
using Tesseract.Domain.Events;
using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;

namespace Tesseract.Domain;

[method: SetsRequiredMembers]
public class CreateRoomEvent(RoomId roomId, UserId creatorId)
    : Event(EventTypes.CreateRoom, roomId, creatorId, string.Empty);