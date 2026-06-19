using Tesseract.Domain.Rooms;

namespace Tesseract.Application.ClientServer.Rooms;

public interface IRoomRepository
{
    Task Save(Room room);
}