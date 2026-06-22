using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

public static class GetCurrentSessionDetails
{
    public sealed record Query(SessionId SessionId) : IRequest<Response>;

    internal sealed class Handler(
        ISessionRepository sessionRepository,
        IUserRepository userRepository)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            if (await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken) is not { } session
                || await userRepository.GetByIdAsync(session.UserId, cancellationToken) is not { } user)
            {
                throw new ForbiddenException();
            }

            return new Response(user.Handle);
        }
    }

    public sealed record Response(UserHandle UserId);
}