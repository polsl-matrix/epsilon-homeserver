using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;

namespace Tesseract.Application.ClientServer.Auth;

public static class RefreshAccessToken
{
    public sealed record Command(string RefreshToken) : IRequest<Response>;

    internal sealed class Handler(
        IHashService hashService,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService,
        ISessionRepository sessionRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var refreshTokenHash = await hashService.HashAsync(request.RefreshToken, cancellationToken);

            if (await sessionRepository.GetByRefreshTokenAsync(refreshTokenHash, cancellationToken) is not { } session)
            {
                throw new UnknownTokenException();
            }

            if (session.PendingAccessTokenHash is not null
                && session.PendingRefreshTokenHash is not null
                && session.PendingRefreshTokenHash.SequenceEqual(refreshTokenHash))
            {
                var promoted = await sessionRepository.PromotePendingTokensAsync(
                    session.Id,
                    session.PendingAccessTokenHash,
                    session.PendingRefreshTokenHash,
                    cancellationToken);

                if (!promoted)
                {
                    throw new UnknownTokenException();
                }
            }

            var (accessToken, accessTokenHash) = await CreateTokenAsync(accessTokenService, cancellationToken);
            var (refreshToken, newRefreshTokenHash) = await CreateTokenAsync(refreshTokenService, cancellationToken);

            await sessionRepository.SetPendingTokensAsync(
                session.Id,
                accessTokenHash,
                newRefreshTokenHash,
                cancellationToken);

            return new Response(accessToken, refreshToken);
        }

        private async Task<(string Token, byte[] Hash)> CreateTokenAsync(
            ITokenService tokenService,
            CancellationToken cancellationToken)
        {
            var token = await tokenService.CreateAsync(cancellationToken);
            var hash = await hashService.HashAsync(token, cancellationToken);
            return (token, hash);
        }
    }

    public sealed record Response(string AccessToken, string RefreshToken);
}