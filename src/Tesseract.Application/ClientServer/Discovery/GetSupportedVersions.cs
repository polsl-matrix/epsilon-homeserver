using MediatR;
using Tesseract.Application.ClientServer.Discovery.Abstractions;

namespace Tesseract.Application.ClientServer.Discovery;

public static class GetSupportedVersions
{
    public record Query : IRequest<Response>;

    internal class Handler(IVersionRepository versionRepository) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var versions = await versionRepository
                .GetSupportedVersions(cancellationToken);

            return new Response(versions.ToList());
        }
    }

    public record Response(IReadOnlyList<string> Versions);
}