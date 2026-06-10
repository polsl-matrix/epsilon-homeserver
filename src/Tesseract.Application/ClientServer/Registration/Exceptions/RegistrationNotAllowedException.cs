namespace Tesseract.Application.ClientServer.Registration.Exceptions;

public sealed class RegistrationNotAllowedException(string message) : Exception(message);