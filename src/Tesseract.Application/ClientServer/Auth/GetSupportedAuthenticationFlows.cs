using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Models;

namespace Tesseract.Application.ClientServer.Auth;

public static class GetSupportedAuthenticationFlows
{
    public sealed record Query : IRequest<Response>;

    internal sealed class Handler(IEnumerable<IAuthenticationFlow> flows) : IRequestHandler<Query, Response>
    {
        public Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var types = flows.Select(it => new LoginFlow(it.Type));
            return Task.FromResult(new Response([..types]));
        }
    }

    public sealed record Response(IReadOnlyList<LoginFlow> Flows);
}