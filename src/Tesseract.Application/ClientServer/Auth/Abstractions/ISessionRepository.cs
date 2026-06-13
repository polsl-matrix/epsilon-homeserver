using Tesseract.Application.ClientServer.Auth.Models;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface ISessionRepository
{
    Task UpsertAsync(Session session, CancellationToken cancellationToken);
}