using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.ClientServer.Auth;

// TODO: Move this file to UseCases directory.
public static class LoginUser
{
    public record Command(string? User, string? Password, string Type) : IRequest<Response>;

    internal sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly Dictionary<string, IAuthenticationFlow> _flows;

        private readonly ISessionFactory _sessionFactory;
        private readonly ISessionRepository _sessionRepository;

        public Handler(IEnumerable<IAuthenticationFlow> flows,
            ISessionFactory sessionFactory, ISessionRepository sessionRepository)
        {
            _flows = flows.ToDictionary(flow => flow.Type);

            _sessionFactory = sessionFactory;
            _sessionRepository = sessionRepository;
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

            var (session, accessToken, refreshToken) = await _sessionFactory
                .CreateAsync(user, cancellationToken);

            await _sessionRepository.UpsertAsync(session, cancellationToken);

            return new Response(user.Handle, accessToken, refreshToken);
        }
    }

    public record Response(UserHandle Handle, string AccessToken, string RefreshToken);
}