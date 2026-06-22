using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Profile;

public static class UpdateAvatarUrl
{
    public sealed record Command(UserId UserId, string UserHandle, string AvatarUrl)
        : IRequest<Response>;

    internal sealed class Handler(IProfileRepository profileRepository, IUserRepository userRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await userRepository.GetByIdAsync(request.UserId, cancellationToken) is not { } user)
            {
                throw new ForbiddenException();
            }

            if (!UserHandle.TryParse(request.UserHandle, out var userHandle) || user.Handle != userHandle)
            {
                throw new CannotUpdateOtherUserProfileException(user.Handle.ToString(), request.UserHandle);
            }

            await profileRepository.UpsertAvatarUrlAsync(request.UserId, request.AvatarUrl, cancellationToken);

            return new Response();
        }
    }

    public sealed record Response;
}