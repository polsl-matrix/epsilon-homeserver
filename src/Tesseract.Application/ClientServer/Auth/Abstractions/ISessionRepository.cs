using Tesseract.Application.ClientServer.Auth.Models;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface ISessionRepository
{
    Task UpsertSessionAsync(Session session, CancellationToken cancellationToken);
}