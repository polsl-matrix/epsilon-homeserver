using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;

namespace Tesseract.Web.Common.Auth;

public interface ICurrentUser
{
    UserId Id { get; }
    SessionId SessionId { get; }
}