using MediatR;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Domain.Support;

namespace Tesseract.Application.ClientServer.Discovery;

public static class GetSupportInfo
{
    public sealed record Query : IRequest<Response>;
    
    public sealed record Response(SupportInfo SupportInfo);

    internal sealed class Handler(ISupportRepository supportRepository) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var supportInfo = await supportRepository.GetSupportInfo(cancellationToken);
            return new Response(supportInfo);
        }
    }
}