namespace Tesseract.Domain.Rooms;

public class Room(RoomId id) : AggregateRoot<RoomId>(id);