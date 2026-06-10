using MediatR;
using Microsoft.Extensions.Options;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Registration.Abstractions;
using Tesseract.Application.ClientServer.Registration.Exceptions;
using Tesseract.Domain.Accounts.Values;

namespace Tesseract.Application.ClientServer.Registration;

public static class RegisterAccount
{
    private const string UserKind = "user";
    private const string LocalpartAllowedSymbols = "._=/+-";
    private const int MaxUserIdLength = 255;

    public sealed record Command(
        string Kind,
        string? Username,
        string? Password,
        string? DeviceId,
        string? InitialDeviceDisplayName,
        bool InhibitLogin,
        AuthenticationData? Auth) : IRequest<Response>;

    public sealed record AuthenticationData(string? Type, string? Session);

    internal sealed class Handler(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher,
        IIdentifierGenerator identifierGenerator,
        IOptions<MatrixOptions> options) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.Kind != UserKind)
            {
                throw new RegistrationNotAllowedException(
                    $"Registration of '{request.Kind}' accounts is not supported.");
            }

            var serverName = options.Value.ServerName;
            if (string.IsNullOrWhiteSpace(serverName))
            {
                throw new InvalidOperationException("Matrix:ServerName configuration is required.");
            }

            if (request.Username is not null)
            {
                ValidateUsername(request.Username, serverName);

                if (await accountRepository.IsLocalpartTaken(request.Username, cancellationToken))
                {
                    throw new UserInUseException("The desired user ID is already taken.");
                }
            }

            if (request.Auth?.Type != AuthenticationTypes.Dummy)
            {
                var session = request.Auth?.Session ?? Guid.NewGuid().ToString("N");

                return new Response.AuthenticationRequired(session, [[AuthenticationTypes.Dummy]]);
            }

            var localpart = request.Username ?? identifierGenerator.GenerateLocalpart();
            var passwordHash = request.Password is null ? null : passwordHasher.Hash(request.Password);

            Device? device = null;
            if (!request.InhibitLogin)
            {
                var deviceId = string.IsNullOrWhiteSpace(request.DeviceId)
                    ? identifierGenerator.GenerateDeviceId()
                    : request.DeviceId;

                device = new Device(
                    deviceId,
                    request.InitialDeviceDisplayName,
                    identifierGenerator.GenerateAccessToken());
            }

            await accountRepository.CreateAccount(
                new Account(localpart, passwordHash), device, cancellationToken);

            return new Response.Registered($"@{localpart}:{serverName}", device?.DeviceId, device?.AccessToken);
        }

        private static void ValidateUsername(string username, string serverName)
        {
            if (username.Length == 0 || !username.All(IsAllowedLocalpartCharacter))
            {
                throw new InvalidUsernameException("The desired user ID is not a valid user name.");
            }

            if ($"@{username}:{serverName}".Length > MaxUserIdLength)
            {
                throw new InvalidUsernameException("The desired user ID is too long.");
            }
        }

        private static bool IsAllowedLocalpartCharacter(char character) =>
            character is >= 'a' and <= 'z' or >= '0' and <= '9'
            || LocalpartAllowedSymbols.Contains(character);
    }

    public abstract record Response
    {
        public sealed record Registered(string UserId, string? DeviceId, string? AccessToken) : Response;

        public sealed record AuthenticationRequired(
            string Session,
            IReadOnlyList<IReadOnlyList<string>> Flows) : Response;
    }
}