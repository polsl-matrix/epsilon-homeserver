using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IPasswordRepository
{
    Task InsertAsync(Password password, CancellationToken cancellationToken);
    Task<Password?> GetAsync(UserId userId, CancellationToken cancellationToken);
}