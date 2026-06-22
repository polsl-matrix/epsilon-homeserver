using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Models;

namespace Tesseract.Application.ClientServer.Auth;

public static class LogoutCurrentUserSession
{
    public sealed record Query(SessionId SessionId) : IRequest<Response>;

    internal sealed class Handler(ISessionRepository sessionRepository) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            await sessionRepository.DeleteByIdAsync(request.SessionId, cancellationToken);

            return new Response();
        }
    }

    public sealed record Response;
}