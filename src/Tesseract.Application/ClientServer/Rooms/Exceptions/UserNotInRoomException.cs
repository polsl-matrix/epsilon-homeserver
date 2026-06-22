using ApplicationException = Tesseract.Application.Common.Exceptions.ApplicationException;

namespace Tesseract.Application.ClientServer.Rooms.Exceptions;

public class UserNotInRoomException()
    : ApplicationException("User does participate in the room.");