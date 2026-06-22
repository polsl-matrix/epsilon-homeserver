namespace Tesseract.Application.ClientServer.Profile.Exceptions;

public sealed class ProfileUpdateForbiddenException(string authenticatedUserId, string targetUserId)
    : Tesseract.Application.Common.Exceptions.ApplicationException(
        $"User '{authenticatedUserId}' cannot update profile for '{targetUserId}'.")
{
    public string AuthenticatedUserId { get; } = authenticatedUserId;
    public string TargetUserId { get; } = targetUserId;
}