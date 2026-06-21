using Tesseract.Domain.Users;

namespace Tesseract.Web.Common.Auth;

public interface ICurrentUser
{
    UserId Id { get; }
}