namespace Tesseract.Application.ClientServer.Registration.Exceptions;

public sealed class UserInUseException(string message) : Exception(message);