using MediatR;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.ClientServer.Auth;

public static class RegisterAccount
{
    private const string UserKind = "user";
    private const string DummyAuthType = "m.login.dummy";

    public sealed record AuthenticationData(string? Type, string? Session);

    public sealed record Command(
        string? Kind,
        string? Username,
        string? Password,
        AuthenticationData? Auth,
        string? DeviceId,
        string? InitialDeviceDisplayName,
        bool InhibitLogin,
        bool RefreshToken)
        : IRequest<Response>;

    internal sealed class Handler(
        IMatrixConfigurationRepository matrixConfigurationRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISessionFactory sessionFactory,
        IAccountRegistrationRepository accountRegistrationRepository,
        ILocalpartGenerator localpartGenerator,
        IDeviceIdGenerator deviceIdGenerator)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.Kind) && request.Kind != UserKind)
            {
                throw new RegistrationForbiddenException("This homeserver only supports user registration.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new MissingParameterException("password");
            }

            var domain = await matrixConfigurationRepository.GetDomainAsync(cancellationToken);
            var localpart = GetLocalpart(request.Username);
            var handle = new UserHandle(localpart.Value, domain.Value);

            await EnsureUserIsAvailableAsync(handle, cancellationToken);

            if (request.Auth?.Type != DummyAuthType || string.IsNullOrWhiteSpace(request.Auth.Session))
            {
                throw CreateAuthenticationRequiredException(request.Auth?.Type);
            }

            await EnsureUserIsAvailableAsync(handle, cancellationToken);

            var user = new User(UserId.Random(), handle);
            var profile = new Profile(user.Id, null, null);
            var passwordHash = await passwordHasher.HashAsync(request.Password, cancellationToken);

            Device? device = null;
            Session? session = null;
            string? accessToken = null;
            string? refreshToken = null;

            if (!request.InhibitLogin)
            {
                var deviceId = string.IsNullOrWhiteSpace(request.DeviceId)
                    ? deviceIdGenerator.Create()
                    : request.DeviceId;

                device = new Device(user.Id, deviceId, request.InitialDeviceDisplayName);
                var createdSession = await sessionFactory.CreateAsync(user, cancellationToken);

                session = createdSession.Session with { DeviceId = deviceId };
                accessToken = createdSession.AccessToken;
                refreshToken = request.RefreshToken ? createdSession.RefreshToken : null;
            }

            var registration = new AccountRegistration(user, profile, passwordHash, device, session);
            await accountRegistrationRepository.CreateAsync(registration, cancellationToken);

            return new Response(handle, accessToken, device?.Id, refreshToken);
        }

        private Localpart GetLocalpart(string? username)
        {
            if (username is null)
            {
                return localpartGenerator.Create();
            }

            if (!Localpart.TryParse(username, out var localpart))
            {
                throw new InvalidUsernameException(username);
            }

            return localpart;
        }

        private async Task EnsureUserIsAvailableAsync(UserHandle handle, CancellationToken cancellationToken)
        {
            if (await userRepository.GetByHandleAsync(handle, cancellationToken) is not null)
            {
                throw new UserInUseException(handle.ToString());
            }
        }

        private UserInteractiveAuthenticationRequiredException CreateAuthenticationRequiredException(string? authType)
        {
            var errorCode = authType is null ? null : "M_FORBIDDEN";
            var error = authType is null ? null : "Unsupported authentication type.";

            return new UserInteractiveAuthenticationRequiredException(
                [[DummyAuthType]],
                Guid.NewGuid().ToString("N"),
                errorCode: errorCode,
                error: error);
        }
    }

    public sealed record Response(UserHandle Handle, string? AccessToken, string? DeviceId, string? RefreshToken);
}