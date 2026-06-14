using Dapper;
using Npgsql;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Auth.Repositories;

internal sealed class DbAccountRegistrationRepository(IDbConnectionFactory dbConnectionFactory)
    : IAccountRegistrationRepository
{
    public async Task CreateAsync(AccountRegistration registration, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            await InsertUserAsync(registration, transaction, cancellationToken);
            await InsertProfileAsync(registration, transaction, cancellationToken);
            await InsertPasswordAsync(registration, transaction, cancellationToken);

            if (registration.Device is not null)
            {
                await InsertDeviceAsync(registration.Device, transaction, cancellationToken);
            }

            if (registration.Session is not null)
            {
                await InsertSessionAsync(registration.Session, transaction, cancellationToken);
            }

            transaction.Commit();
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            transaction.Rollback();
            throw new UserInUseException(registration.User.Handle.ToString());
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static Task InsertUserAsync(
        AccountRegistration registration,
        System.Data.IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO identity.users(user_id, localpart, domain)
                           VALUES (@UserId, @Localpart, @Domain);
                           """;

        return transaction.Connection!.ExecuteAsync(new CommandDefinition(sql, new
        {
            UserId = registration.User.Id.Value,
            Localpart = registration.User.Handle.Localpart.Value,
            Domain = registration.User.Handle.Domain.Value,
        }, transaction, cancellationToken: cancellationToken));
    }

    private static Task InsertProfileAsync(
        AccountRegistration registration,
        System.Data.IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO identity.profiles(user_id, display_name, avatar_url)
                           VALUES (@UserId, @DisplayName, @AvatarUrl);
                           """;

        return transaction.Connection!.ExecuteAsync(new CommandDefinition(sql, new
        {
            UserId = registration.Profile.UserId.Value,
            registration.Profile.DisplayName,
            registration.Profile.AvatarUrl,
        }, transaction, cancellationToken: cancellationToken));
    }

    private static Task InsertPasswordAsync(
        AccountRegistration registration,
        System.Data.IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO auth.passwords(user_id, password_hash)
                           VALUES (@UserId, @PasswordHash);
                           """;

        return transaction.Connection!.ExecuteAsync(new CommandDefinition(sql, new
        {
            UserId = registration.User.Id.Value,
            registration.PasswordHash,
        }, transaction, cancellationToken: cancellationToken));
    }

    private static Task InsertDeviceAsync(
        Device device,
        System.Data.IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO auth.devices(user_id, device_id, display_name)
                           VALUES (@UserId, @DeviceId, @DisplayName);
                           """;

        return transaction.Connection!.ExecuteAsync(new CommandDefinition(sql, new
        {
            UserId = device.UserId.Value,
            DeviceId = device.Id,
            device.DisplayName,
        }, transaction, cancellationToken: cancellationToken));
    }

    private static Task InsertSessionAsync(
        Session session,
        System.Data.IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO auth.sessions(session_id, user_id, device_id, current_access_token_hash, current_refresh_token_hash)
                           VALUES (@SessionId, @UserId, @DeviceId, @CurrentAccessTokenHash, @CurrentRefreshTokenHash);
                           """;

        return transaction.Connection!.ExecuteAsync(new CommandDefinition(sql, new
        {
            SessionId = session.Id.Value,
            UserId = session.UserId.Value,
            session.DeviceId,
            CurrentAccessTokenHash = session.AccessTokenHash,
            CurrentRefreshTokenHash = session.RefreshTokenHash,
        }, transaction, cancellationToken: cancellationToken));
    }
}