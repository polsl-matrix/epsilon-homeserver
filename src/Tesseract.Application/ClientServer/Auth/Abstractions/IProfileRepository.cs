using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IProfileRepository
{
    Task InsertAsync(Tesseract.Domain.Users.Profile profile, CancellationToken cancellationToken);
    Task UpsertDisplayNameAsync(UserHandle handle, string? displayName, CancellationToken cancellationToken);
}