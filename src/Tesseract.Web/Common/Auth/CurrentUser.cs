using System.Security.Claims;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;

namespace Tesseract.Web.Common.Auth;

internal class CurrentUser(IHttpContextAccessor context) : ICurrentUser
{
    public UserId Id => new(Guid.Parse(RequireClaim(ClaimTypes.NameIdentifier)));
    public SessionId SessionId => new(Guid.Parse(RequireClaim(ClaimTypes.Sid)));

    private string RequireClaim(string type)
    {
        if (GetUser().FindFirstValue(type) is not { } claim)
        {
            throw new InvalidOperationException("Required claim is missing.");
        }

        return claim;
    }

    private ClaimsPrincipal GetUser()
    {
        if (context.HttpContext?.User is not { Identity.IsAuthenticated: true } user)
        {
            throw new UnauthorizedAccessException("User is not logged in.");
        }

        return user;
    }
}