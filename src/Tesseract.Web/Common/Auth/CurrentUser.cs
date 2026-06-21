using System.Security.Claims;
using Tesseract.Domain.Users;

namespace Tesseract.Web.Common.Auth;

public class CurrentUser(IHttpContextAccessor context) : ICurrentUser
{
    public UserId Id
    {
        get
        {
            var claim = FindClaim(ClaimTypes.NameIdentifier);
            return new UserId(Guid.Parse(claim ?? string.Empty));
        }
    }

    private string? FindClaim(string type) =>
        GetUser().FindFirstValue(type);

    private ClaimsPrincipal GetUser()
    {
        if (context.HttpContext?.User is not { } user)
        {
            throw new UnauthorizedAccessException("User is not logged in.");
        }

        return user;
    }
}