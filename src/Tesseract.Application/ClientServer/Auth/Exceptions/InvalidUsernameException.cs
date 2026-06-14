namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public sealed class InvalidUsernameException(string? username)
    : Exception($"Invalid username: {username}.")
{
    public string? Username { get; } = username;
}