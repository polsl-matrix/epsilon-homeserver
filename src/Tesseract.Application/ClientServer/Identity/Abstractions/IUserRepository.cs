using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Identity.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken);
}