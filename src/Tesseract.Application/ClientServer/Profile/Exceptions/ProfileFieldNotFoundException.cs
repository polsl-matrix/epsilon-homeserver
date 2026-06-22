using ApplicationException = Tesseract.Application.Common.Exceptions.ApplicationException;

namespace Tesseract.Application.ClientServer.Profile.Exceptions;

public sealed class ProfileFieldNotFoundException(string userId, string field)
    : ApplicationException($"Profile field '{field}' was not found for user '{userId}'.")
{
    public string UserId { get; } = userId;

    public string Field { get; } = field;
}