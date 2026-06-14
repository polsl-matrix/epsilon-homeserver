using Dapper;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Infrastructure.ClientServer.Auth.Dao;
using Tesseract.Infrastructure.ClientServer.Auth.Mappers;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Auth.Repositories;

internal class DbSessionRepository(IDbConnectionFactory dbConnectionFactory) : ISessionRepository
{
    public async Task<Session?> GetByAccessTokenAsync(byte[] accessTokenHash, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                             SELECT session_id                 {nameof(SessionDao.SessionId)},
                                    user_id                    {nameof(SessionDao.UserId)},
                                    current_access_token_hash  {nameof(SessionDao.CurrentAccessTokenHash)},
                                    current_refresh_token_hash {nameof(SessionDao.CurrentRefreshTokenHash)},
                                    pending_access_token_hash  {nameof(SessionDao.PendingAccessTokenHash)},
                                    pending_refresh_token_hash {nameof(SessionDao.PendingRefreshTokenHash)}
                             FROM auth.sessions
                             WHERE current_access_token_hash = @AccessTokenHash
                                OR pending_access_token_hash = @AccessTokenHash;
                             """;

        var parameters = new
        {
            AccessTokenHash = accessTokenHash,
        };

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        if (await connection.QuerySingleOrDefaultAsync<SessionDao>(command) is not { } sessionDao)
        {
            return null;
        }

        return sessionDao.ToDomain();
    }

    public async Task<Session?> GetByRefreshTokenAsync(byte[] refreshTokenHash, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                             SELECT session_id                 {nameof(SessionDao.SessionId)},
                                    user_id                    {nameof(SessionDao.UserId)},
                                    current_access_token_hash  {nameof(SessionDao.CurrentAccessTokenHash)},
                                    current_refresh_token_hash {nameof(SessionDao.CurrentRefreshTokenHash)},
                                    pending_access_token_hash  {nameof(SessionDao.PendingAccessTokenHash)},
                                    pending_refresh_token_hash {nameof(SessionDao.PendingRefreshTokenHash)}
                             FROM auth.sessions
                             WHERE current_refresh_token_hash = @RefreshTokenHash
                                OR pending_refresh_token_hash = @RefreshTokenHash;
                             """;

        var parameters = new
        {
            RefreshTokenHash = refreshTokenHash,
        };

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        if (await connection.QuerySingleOrDefaultAsync<SessionDao>(command) is not { } sessionDao)
        {
            return null;
        }

        return sessionDao.ToDomain();
    }

    public async Task<bool> PromotePendingTokensByAccessTokenAsync(
        byte[] accessTokenHash, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           UPDATE auth.sessions
                           SET current_access_token_hash = pending_access_token_hash,
                               current_refresh_token_hash = pending_refresh_token_hash,
                               pending_access_token_hash = NULL,
                               pending_refresh_token_hash = NULL
                           WHERE pending_access_token_hash = @AccessTokenHash;
                           """;

        var parameters = new
        {
            AccessTokenHash = accessTokenHash,
        };

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command) > 0;
    }

    public async Task<bool> RotateRefreshTokenAsync(
        byte[] refreshTokenHash,
        byte[] pendingAccessTokenHash,
        byte[] pendingRefreshTokenHash,
        CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           UPDATE auth.sessions
                           SET current_access_token_hash = CASE
                                   WHEN pending_refresh_token_hash = @RefreshTokenHash THEN pending_access_token_hash
                                   ELSE current_access_token_hash
                               END,
                               current_refresh_token_hash = CASE
                                   WHEN pending_refresh_token_hash = @RefreshTokenHash THEN pending_refresh_token_hash
                                   ELSE current_refresh_token_hash
                               END,
                               pending_access_token_hash = @PendingAccessTokenHash,
                               pending_refresh_token_hash = @PendingRefreshTokenHash
                           WHERE current_refresh_token_hash = @RefreshTokenHash
                              OR pending_refresh_token_hash = @RefreshTokenHash;
                           """;

        var parameters = new
        {
            RefreshTokenHash = refreshTokenHash,
            PendingAccessTokenHash = pendingAccessTokenHash,
            PendingRefreshTokenHash = pendingRefreshTokenHash,
        };

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command) > 0;
    }

    public async Task UpsertAsync(Session session, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO auth.sessions(
                               session_id,
                               user_id,
                               current_access_token_hash,
                               current_refresh_token_hash,
                               pending_access_token_hash,
                               pending_refresh_token_hash)
                           VALUES (
                               @SessionId,
                               @UserId,
                               @CurrentAccessTokenHash,
                               @CurrentRefreshTokenHash,
                               @PendingAccessTokenHash,
                               @PendingRefreshTokenHash)
                           ON CONFLICT (session_id) DO UPDATE
                           SET current_access_token_hash = EXCLUDED.current_access_token_hash,
                               current_refresh_token_hash = EXCLUDED.current_refresh_token_hash,
                               pending_access_token_hash = EXCLUDED.pending_access_token_hash,
                               pending_refresh_token_hash = EXCLUDED.pending_refresh_token_hash;
                           """;

        var parameters = new
        {
            SessionId = session.Id.Value,
            UserId = session.UserId.Value,
            CurrentAccessTokenHash = session.CurrentAccessTokenHash,
            CurrentRefreshTokenHash = session.CurrentRefreshTokenHash,
            PendingAccessTokenHash = session.PendingAccessTokenHash,
            PendingRefreshTokenHash = session.PendingRefreshTokenHash,
        };

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}