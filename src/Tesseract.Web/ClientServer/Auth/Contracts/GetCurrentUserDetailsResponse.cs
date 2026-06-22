namespace Tesseract.Web.ClientServer.Auth.Contracts;

public sealed class GetCurrentUserDetailsResponse
{
    public required string UserId { get; init; }
}