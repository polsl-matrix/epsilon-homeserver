using ApplicationException = Tesseract.Application.Common.Exceptions.ApplicationException;

namespace Tesseract.Application.ClientServer.Rooms.Exceptions;

public sealed class RoomNotFoundException(string handle)
    : ApplicationException($"Room not found: {handle}")
{
    public string Handle { get; init; } = handle;
}