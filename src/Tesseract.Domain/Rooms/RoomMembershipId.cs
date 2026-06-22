using Tesseract.Domain.Users;

namespace Tesseract.Domain.Rooms;

public readonly record struct RoomMembershipId(RoomId RoomId, UserId UserId);