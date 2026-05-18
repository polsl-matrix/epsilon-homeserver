using MediatR;
using Tesseract.Application.ClientServer.Discovery.Abstractions;

namespace Tesseract.Application.ClientServer.Discovery;

public static class GetDomainDiscovery
{
    public sealed record Query : IRequest<Response?>;

    internal sealed class Handler(IWellKnownRepository repository) : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query request, CancellationToken cancellationToken)
        {
            var info = await repository.GetDiscoveryInfo(cancellationToken);

            if (info is null)
            {
                return null;
            }

            return new Response(info.HomeserverBaseUrl, info.IdentityServerBaseUrl);
        }
    }

    public sealed record Response(string HomeserverBaseUrl, string? IdentityServerBaseUrl);
}