using MediatR;
using Tesseract.Application.ClientServer.Discovery.Abstractions;

namespace Tesseract.Application.ClientServer.Discovery;

public static class GetDomainDiscovery
{
    public sealed record Query : IRequest<Response>;

    internal sealed class Handler(IWellKnownRepository repository) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var info = await repository.GetDiscoveryInfo(cancellationToken);

            if (info is null)
            {
                throw new InvalidOperationException("Discovery information is not configured.");
            }

            var homeserverBaseUrl = info.HomeserverBaseUrl.ToString();
            var identityServerBaseUrl = info.IdentityServerBaseUrl?.ToString();

            return new Response(homeserverBaseUrl, identityServerBaseUrl);
        }
    }

    public sealed record Response(string HomeserverBaseUrl, string? IdentityServerBaseUrl);
}