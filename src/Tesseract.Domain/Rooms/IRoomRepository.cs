namespace Tesseract.Domain.Rooms;

public interface IRoomRepository
{
    Task Save(Room room);
}