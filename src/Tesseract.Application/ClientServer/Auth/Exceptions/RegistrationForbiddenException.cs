namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public sealed class RegistrationForbiddenException(string reason)
    : Exception(reason);