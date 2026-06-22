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
        private readonly Dictionary<string, IAuthenticationFlow> _authenticationFlows;

        private readonly ISessionFactory _sessionFactory;
        private readonly ISessionRepository _sessionRepository;

        public Handler(IEnumerable<IAuthenticationFlow> flows,
            ISessionFactory sessionFactory, ISessionRepository sessionRepository)
        {
            _authenticationFlows = flows.ToDictionary(flow => flow.Type);

            _sessionFactory = sessionFactory;
            _sessionRepository = sessionRepository;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!_authenticationFlows.TryGetValue(request.Type, out var authenticationFlow))
            {
                throw new BadLoginTypeException(request.Type);
            }

            if (await authenticationFlow.AuthenticateAsync(request.User, request.Password, cancellationToken)
                is not { Deactivated: false } user)
            {
                throw new ForbiddenException();
            }

            var (session, accessToken, refreshToken) = await _sessionFactory
                .CreateAsync(user, cancellationToken);

            await _sessionRepository.UpsertAsync(session, cancellationToken);

            return new Response(user.Handle, accessToken, refreshToken);
        }
    }

    public record Response(UserHandle Handle, string AccessToken, string RefreshToken);
}