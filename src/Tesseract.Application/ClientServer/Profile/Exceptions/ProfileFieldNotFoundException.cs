namespace Tesseract.Application.ClientServer.Profile.Exceptions;

public sealed class ProfileFieldNotFoundException(string userId, string field)
    : Tesseract.Application.Common.Exceptions.ApplicationException(
        $"Profile field '{field}' was not found for user '{userId}'.")
{
    public string UserId { get; } = userId;
    public string Field { get; } = field;
}