using Tesseract.Domain.Rooms;
using Tesseract.Domain.Users;
using VDomain = Tesseract.Domain.Common.Values.Domain;

namespace Tesseract.Domain.Events;

public sealed class CreateRoomEvent(Room room, User creator, VDomain domain)
    : Event(EventTypes.CreateRoom, room, creator, string.Empty, domain);