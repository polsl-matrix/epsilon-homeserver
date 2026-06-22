using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

public static class DeactivateAccount
{
    public sealed record Command(UserId UserId) : IRequest<Response>;

    internal sealed class Handler(IUserRepository userRepository, ISessionRepository sessionRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            await userRepository.MarkDeactivatedAsync(request.UserId, cancellationToken);
            await sessionRepository.DeleteAllByUserIdAsync(request.UserId, cancellationToken);

            return new Response("no-support");
        }
    }

    public sealed record Response(string IdServerUnbindResult);
}