using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Identity.Abstractions;

public interface IUserRepository
{
    Task InsertAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken);
    Task<User?> GetByHandleAsync(UserHandle handle, CancellationToken cancellationToken);
    Task MarkDeactivatedAsync(UserId id, CancellationToken cancellationToken);
}