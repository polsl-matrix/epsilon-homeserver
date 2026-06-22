using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

public static class LogoutAllUserSessions
{
    public sealed record Query(UserId UserId) : IRequest<Response>;

    internal sealed class Handler(ISessionRepository sessionRepository) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            await sessionRepository.DeleteAllByUserIdAsync(request.UserId, cancellationToken);

            return new Response();
        }
    }

    public sealed record Response;
}