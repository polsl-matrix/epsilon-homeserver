using ApplicationException = Tesseract.Application.Common.Exceptions.ApplicationException;

namespace Tesseract.Application.ClientServer.Profile.Exceptions;

public sealed class CannotUpdateOtherUserProfileException(string authenticatedUserId, string targetUserId)
    : ApplicationException(
        $"User '{authenticatedUserId}' cannot update profile for '{targetUserId}'.")
{
    public string AuthenticatedUserId { get; } = authenticatedUserId;
    public string TargetUserId { get; } = targetUserId;
}