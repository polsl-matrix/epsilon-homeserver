using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IPasswordRepository
{
    Task<Password?> GetHashAsync(UserId userId, CancellationToken cancellationToken);
}