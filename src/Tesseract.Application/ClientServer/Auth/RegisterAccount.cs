using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Application.Common.Transactions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.ClientServer.Auth;

public static class RegisterAccount
{
    public sealed record Command(string Username, string Password) : IRequest<Response>, ITransactional;

    internal sealed class Handler(
        IPasswordHasher passwordHasher,
        IMatrixConfigurationRepository configurationRepository,
        IPasswordRepository passwordRepository,
        IProfileRepository profileRepository,
        ISessionFactory sessionFactory,
        ISessionRepository sessionRepository,
        IUserRepository userRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var domain = await configurationRepository
                .GetDomainAsync(cancellationToken);

            var handle = new UserHandle(request.Username, domain.Value);

            await EnsureUsernameNotTakenAsync(handle, cancellationToken);

            var user = new User(UserId.Random(), handle);
            var profile = Domain.Users.Profile.Empty(user.Id);

            var password = await HashPassword(user.Id, request.Password, cancellationToken);

            await userRepository.InsertAsync(user, cancellationToken);
            await profileRepository.InsertAsync(profile, cancellationToken);
            await passwordRepository.InsertAsync(password, cancellationToken);

            var (session, accessToken, refreshToken) = await sessionFactory
                .CreateAsync(user, cancellationToken);

            await sessionRepository.UpsertAsync(session, cancellationToken);

            return new Response(user.Handle, accessToken, refreshToken);
        }

        private async Task EnsureUsernameNotTakenAsync(UserHandle handle, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByHandleAsync(handle, cancellationToken);

            if (user is not null)
            {
                throw new UsernameTakenException(handle.Localpart.Value);
            }
        }

        private async Task<Password> HashPassword(
            UserId userId, string password, CancellationToken cancellationToken)
        {
            var hash = await passwordHasher
                .HashAsync(password, cancellationToken);

            return new Password(userId, hash);
        }
    }

    public sealed record Response(UserHandle Handle, string AccessToken, string RefreshToken);
}