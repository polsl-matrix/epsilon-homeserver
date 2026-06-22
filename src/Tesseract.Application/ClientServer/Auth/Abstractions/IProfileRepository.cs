namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface IProfileRepository
{
    Task InsertAsync(Tesseract.Domain.Users.Profile profile, CancellationToken cancellationToken);

    Task<string?> GetDisplayNameAsync(Tesseract.Domain.Users.UserHandle handle, CancellationToken cancellationToken);
}