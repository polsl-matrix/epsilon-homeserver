using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

public static class AuthenticateUser
{
    public record Command(string AccessToken) : IRequest<Response>;

    internal sealed class Handler(
        IHashService hashService,
        ISessionRepository sessionRepository,
        IUserRepository userRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var accessTokenHash = await hashService.HashAsync(request.AccessToken, cancellationToken);

            if (await sessionRepository.GetByAccessTokenAsync(accessTokenHash, cancellationToken) is not { } session)
            {
                return new Response(null);
            }

            if (session.HasPendingAccessTokenHash(accessTokenHash))
            {
                if (!await sessionRepository.PromotePendingTokensByAccessTokenAsync(accessTokenHash, cancellationToken) &&
                    await sessionRepository.GetByAccessTokenAsync(accessTokenHash, cancellationToken) is not { } refreshedSession)
                {
                    return new Response(null);
                }
            }

            var user = await userRepository.GetByIdAsync(session.UserId, cancellationToken);

            return new Response(user);
        }
    }

    public record Response(User? User);
}