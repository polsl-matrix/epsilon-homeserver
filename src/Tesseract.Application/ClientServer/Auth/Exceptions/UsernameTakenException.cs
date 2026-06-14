using ApplicationException = Tesseract.Application.Common.Exceptions.ApplicationException;

namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public sealed class UsernameTakenException(string userId)
    : ApplicationException($"This username is already taken: {userId}")
{
    public string UserId { get; } = userId;
}