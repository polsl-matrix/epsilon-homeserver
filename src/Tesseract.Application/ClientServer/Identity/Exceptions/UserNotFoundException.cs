namespace Tesseract.Application.ClientServer.Identity.Exceptions;

public sealed class UserNotFoundException(string handle)
    : ApplicationException($"User not found: {handle}")
{
    public string Handle { get; init; } = handle;
}