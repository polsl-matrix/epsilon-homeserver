using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

public static class LoginUser
{
    public record Command(string? User, string? Password, string Type) : IRequest<Response>;

    internal sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly Dictionary<string, IAuthenticationFlow> _flows;

        private readonly IAccessTokenService _accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public Handler(IEnumerable<IAuthenticationFlow> flows,
            IAccessTokenService accessTokenService, IRefreshTokenService refreshTokenService)
        {
            _flows = flows.ToDictionary(flow => flow.Type);

            _accessTokenService = accessTokenService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!_flows.TryGetValue(request.Type, out var flow))
            {
                throw new BadLoginTypeException(request.Type);
            }

            if (await flow.AuthenticateAsync(request.User, request.Password, cancellationToken) is not { } user)
            {
                throw new ForbiddenException();
            }

            var (accessToken, refreshToken) = await CreateTokenPair(cancellationToken);

            // TODO: Persist tokens in session (repository).

            return new Response(user, accessToken, refreshToken);
        }

        private async Task<(string, string)> CreateTokenPair(CancellationToken cancellationToken)
        {
            var access = await _accessTokenService.Create(cancellationToken);
            var refresh = await _refreshTokenService.Create(cancellationToken);

            return (access, refresh);
        }
    }

    public record Response(User User, string AccessToken, string RefreshToken);
}