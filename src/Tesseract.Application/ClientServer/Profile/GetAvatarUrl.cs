using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Profile;

public static class GetAvatarUrl
{
    public sealed record Query(string UserId) : IRequest<Response>;

    internal sealed class Handler(IProfileRepository profileRepository) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!UserHandle.TryParse(request.UserId, out var handle))
            {
                throw new ProfileFieldNotFoundException(request.UserId, "avatar_url");
            }

            var avatarUrl = await profileRepository.GetAvatarUrlAsync(handle, cancellationToken);

            if (avatarUrl is null)
            {
                throw new ProfileFieldNotFoundException(request.UserId, "avatar_url");
            }

            return new Response(avatarUrl);
        }
    }

    public sealed record Response(string AvatarUrl);
}