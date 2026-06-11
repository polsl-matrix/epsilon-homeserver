using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Domain.Users.Entities;

namespace Tesseract.Application.ClientServer.Auth;

public static class LoginUser
{
    public record Command(string? User, string? Password, string Type) : IRequest<Response>;

    internal sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly Dictionary<string, IAuthenticationFlow> _flows;

        public Handler(IEnumerable<IAuthenticationFlow> flows)
        {
            _flows = flows.ToDictionary(flow => flow.Type);
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

            // TODO: Persist tokens in session (repository).

            return new Response(user);
        }
    }

    public record Response(User User);
}