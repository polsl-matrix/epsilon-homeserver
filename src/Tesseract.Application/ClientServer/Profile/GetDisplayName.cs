using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Profile;

public static class GetDisplayName
{
    public sealed record Query(string UserId) : IRequest<Response>;

    public sealed record Response(string DisplayName);

    internal sealed class Handler(IProfileRepository profileRepository) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!UserHandle.TryParse(request.UserId, out var handle))
            {
                throw new ProfileFieldNotFoundException(request.UserId, "displayname");
            }

            var displayName = await profileRepository.GetDisplayNameAsync(handle, cancellationToken);

            if (displayName is null)
            {
                throw new ProfileFieldNotFoundException(request.UserId, "displayname");
            }

            return new Response(displayName);
        }
    }
}