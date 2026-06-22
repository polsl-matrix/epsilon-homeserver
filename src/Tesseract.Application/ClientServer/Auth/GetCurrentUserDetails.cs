using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

public static class GetCurrentUserDetails
{
    public sealed record Query(string AccessToken) : IRequest<Response>;

    internal sealed class Handler(
        IHashService hashService,
        ISessionRepository sessionRepository,
        IUserRepository userRepository)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var accessTokenHash = await hashService.HashAsync(request.AccessToken, cancellationToken);

            if (await sessionRepository.GetByAccessTokenAsync(accessTokenHash, cancellationToken) is not { } session)
            {
                throw new UnknownAccessTokenException();
            }

            if (await userRepository.GetByIdAsync(session.UserId, cancellationToken) is not { } user)
            {
                throw new UnknownAccessTokenException();
            }

            return new Response(user.Handle);
        }
    }

    public sealed record Response(UserHandle UserId);
}