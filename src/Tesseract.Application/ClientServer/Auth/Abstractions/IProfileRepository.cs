using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IProfileRepository
{
    Task InsertAsync(Domain.Users.Profile profile, CancellationToken cancellationToken);
    Task UpsertDisplayNameAsync(UserId userId, string displayName, CancellationToken cancellationToken);
    Task UpsertAvatarUrlAsync(UserId userId, string avatarUrl, CancellationToken cancellationToken);
    Task<Domain.Users.Profile?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);
}