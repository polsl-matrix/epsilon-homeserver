using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;

namespace Tesseract.Application.ClientServer.Auth;

public static class RefreshAccessToken
{
    public record Command(string RefreshToken) : IRequest<Response>;

    internal sealed class Handler(
        IHashService hashService,
        ISessionRepository sessionRepository,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var refreshTokenHash = await hashService.HashAsync(request.RefreshToken, cancellationToken);

            if (await sessionRepository.GetByRefreshTokenAsync(refreshTokenHash, cancellationToken) is not { } session)
            {
                throw new UnknownTokenException();
            }

            var accessToken = await accessTokenService.CreateAsync(cancellationToken);
            var newRefreshToken = await refreshTokenService.CreateAsync(cancellationToken);
            var accessTokenHash = await hashService.HashAsync(accessToken, cancellationToken);
            var newRefreshTokenHash = await hashService.HashAsync(newRefreshToken, cancellationToken);

            if (!await sessionRepository.RotateRefreshTokenAsync(
                    refreshTokenHash,
                    accessTokenHash,
                    newRefreshTokenHash,
                    cancellationToken))
            {
                throw new UnknownTokenException();
            }

            return new Response(accessToken, newRefreshToken);
        }
    }

    public record Response(string AccessToken, string RefreshToken);
}