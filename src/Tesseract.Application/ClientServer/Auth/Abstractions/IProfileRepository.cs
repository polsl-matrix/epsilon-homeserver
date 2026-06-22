using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IProfileRepository
{
    Task InsertAsync(Domain.Users.Profile profile, CancellationToken cancellationToken);
    Task UpsertAvatarUrlAsync(UserId userId, string avatarUrl, CancellationToken cancellationToken);
}