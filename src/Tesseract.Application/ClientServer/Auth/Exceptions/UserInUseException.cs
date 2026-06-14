namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public sealed class UserInUseException(string userId)
    : Exception($"Desired user ID is already taken: {userId}.")
{
    public string UserId { get; } = userId;
}