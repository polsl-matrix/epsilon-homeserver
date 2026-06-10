namespace Tesseract.Application.ClientServer.Registration.Exceptions;

public sealed class InvalidUsernameException(string message) : Exception(message);