using ApplicationException = Tesseract.Application.Common.Exceptions.ApplicationException;

namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public sealed class InvalidUsernameException(string? username)
    : ApplicationException($"Invalid username: {username}")
{
    public string? Username { get; } = username;
}