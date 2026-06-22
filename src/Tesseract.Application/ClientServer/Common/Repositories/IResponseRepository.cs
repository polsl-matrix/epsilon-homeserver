using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Common.Repositories;

public interface IResponseRepository
{
    Task<Response?> GetByUserIdAndPathAsync(UserId userId, string path, CancellationToken cancellationToken);

    Task UpsertAsync(Response response, CancellationToken cancellationToken);
}