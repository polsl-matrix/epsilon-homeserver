using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;

namespace Tesseract.Application.ClientServer.Auth;

public static class LogoutUser
{
    public record Command(string AccessToken) : IRequest<Response>;

    internal sealed class Handler(IHashService hashService, ISessionRepository sessionRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var accessTokenHash = await hashService.HashAsync(request.AccessToken, cancellationToken);

            await sessionRepository.DeleteByAccessTokenAsync(accessTokenHash, cancellationToken);

            return new Response();
        }
    }

    public record Response;
}