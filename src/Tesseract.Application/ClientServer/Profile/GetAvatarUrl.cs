using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Identity.Exceptions;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Profile;

public static class GetAvatarUrl
{
    public sealed record Query(string UserHandle) : IRequest<Response>;

    internal sealed class Handler(
        IProfileRepository profileRepository,
        IUserRepository userRepository)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!UserHandle.TryParse(request.UserHandle, out var handle)
                || await userRepository.GetByHandleAsync(handle, cancellationToken) is not { } user)
            {
                throw new UserNotFoundException(request.UserHandle);
            }

            if (await profileRepository.GetAvatarUrlAsync(user.Id, cancellationToken) is not { } avatarUrl)
            {
                throw new ProfileFieldNotFoundException(request.UserHandle, "avatar_url");
            }

            return new Response(avatarUrl);
        }
    }

    public sealed record Response(string AvatarUrl);
}