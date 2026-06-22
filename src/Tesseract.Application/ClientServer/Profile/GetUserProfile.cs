using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Identity.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Profile;

public class GetUserProfile
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
                || await userRepository.GetByHandleAsync(handle, cancellationToken) is not { } user
                || await profileRepository.GetByUserIdAsync(user.Id, cancellationToken) is not { } profile)
            {
                throw new UserNotFoundException(request.UserHandle);
            }

            return new Response(profile.DisplayName, profile.AvatarUrl);
        }
    }

    public sealed record Response(string? DisplayName, string? AvatarUrl);
}