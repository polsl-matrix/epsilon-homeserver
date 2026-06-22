using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Profile;

public static class UpdateDisplayName
{
    public sealed record Command(UserId AuthenticatedUserId, string UserId, string? DisplayName) : IRequest;

    internal sealed class Handler(IProfileRepository profileRepository, IUserRepository userRepository)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.AuthenticatedUserId, cancellationToken)
                ?? throw new ForbiddenException();

            if (!UserHandle.TryParse(request.UserId, out var targetHandle) || user.Handle != targetHandle)
            {
                throw new ProfileUpdateForbiddenException(user.Handle.ToString(), request.UserId);
            }

            await profileRepository.UpsertDisplayNameAsync(targetHandle, request.DisplayName, cancellationToken);
        }
    }
}