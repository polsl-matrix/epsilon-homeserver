using Dapper;
using Npgsql;
using Tesseract.Application.ClientServer.Registration.Abstractions;
using Tesseract.Application.ClientServer.Registration.Exceptions;
using Tesseract.Domain.Accounts.Values;
using Tesseract.Infrastructure.Common.Database;

namespace Tesseract.Infrastructure.ClientServer.Registration;

public class AccountRepository(IDbConnectionFactory connectionFactory) : IAccountRepository
{
    public async Task<bool> IsLocalpartTaken(string localpart, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            "SELECT EXISTS (SELECT 1 FROM users WHERE localpart = @Localpart);",
            new { Localpart = localpart },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task CreateAccount(Account account, Device? device, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT INTO users (localpart, password_hash) VALUES (@Localpart, @PasswordHash);",
                new { account.Localpart, account.PasswordHash },
                transaction,
                cancellationToken: cancellationToken));

            if (device is not null)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    INSERT INTO devices (user_localpart, device_id, display_name, access_token)
                    VALUES (@Localpart, @DeviceId, @DisplayName, @AccessToken);
                    """,
                    new { account.Localpart, device.DeviceId, device.DisplayName, device.AccessToken },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            transaction.Commit();
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new UserInUseException("The desired user ID is already taken.");
        }
    }
}