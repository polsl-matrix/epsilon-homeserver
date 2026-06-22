using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

public static class LogoutAllUserSessions
{
    public sealed record Command(UserId UserId) : IRequest;

    internal sealed class Handler(ISessionRepository sessionRepository) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await sessionRepository.DeleteAllByUserIdAsync(request.UserId, cancellationToken);
        }
    }
}