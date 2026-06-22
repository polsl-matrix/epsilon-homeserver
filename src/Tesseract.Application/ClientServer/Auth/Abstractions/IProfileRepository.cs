using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IProfileRepository
{
    Task InsertAsync(Tesseract.Domain.Users.Profile profile, CancellationToken cancellationToken);
    Task<string?> GetAvatarUrlAsync(UserHandle handle, CancellationToken cancellationToken);
}