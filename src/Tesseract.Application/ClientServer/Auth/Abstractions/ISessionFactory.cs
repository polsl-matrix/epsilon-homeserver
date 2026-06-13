using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface ISessionFactory
{
    Task<(Session, string AccessToken, string RefreshToken)>
        CreateAsync(User user, CancellationToken cancellationToken);
}