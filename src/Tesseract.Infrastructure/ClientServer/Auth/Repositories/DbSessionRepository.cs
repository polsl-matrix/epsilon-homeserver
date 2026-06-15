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
                                   NULL::BYTEA                {nameof(SessionDao.PendingAccessTokenHash)},
                                   NULL::BYTEA                {nameof(SessionDao.PendingRefreshTokenHash)}
                            FROM auth.sessions
                            WHERE current_access_token_hash = @AccessTokenHash

                            UNION ALL

                            SELECT sessions.session_id                         {nameof(SessionDao.SessionId)},
                                   sessions.user_id                            {nameof(SessionDao.UserId)},
                                   sessions.current_access_token_hash          {nameof(SessionDao.CurrentAccessTokenHash)},
                                   sessions.current_refresh_token_hash         {nameof(SessionDao.CurrentRefreshTokenHash)},
                                   pending.pending_access_token_hash           {nameof(SessionDao.PendingAccessTokenHash)},
                                   pending.pending_refresh_token_hash          {nameof(SessionDao.PendingRefreshTokenHash)}
                            FROM auth.sessions sessions
                            INNER JOIN auth.session_pending_tokens pending ON pending.session_id = sessions.session_id
                            WHERE pending.pending_access_token_hash = @AccessTokenHash;
                            """;

        var parameters = new
        {
            AccessTokenHash = accessTokenHash,
        };

        if (await connection.QuerySingleOrDefaultAsync<SessionDao>(sql, parameters) is not { } sessionDao)
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
                                   NULL::BYTEA                {nameof(SessionDao.PendingAccessTokenHash)},
                                   NULL::BYTEA                {nameof(SessionDao.PendingRefreshTokenHash)}
                            FROM auth.sessions
                            WHERE current_refresh_token_hash = @RefreshTokenHash

                            UNION ALL

                            SELECT sessions.session_id                         {nameof(SessionDao.SessionId)},
                                   sessions.user_id                            {nameof(SessionDao.UserId)},
                                   sessions.current_access_token_hash          {nameof(SessionDao.CurrentAccessTokenHash)},
                                   sessions.current_refresh_token_hash         {nameof(SessionDao.CurrentRefreshTokenHash)},
                                   pending.pending_access_token_hash           {nameof(SessionDao.PendingAccessTokenHash)},
                                   pending.pending_refresh_token_hash          {nameof(SessionDao.PendingRefreshTokenHash)}
                            FROM auth.sessions sessions
                            INNER JOIN auth.session_pending_tokens pending ON pending.session_id = sessions.session_id
                            WHERE pending.pending_refresh_token_hash = @RefreshTokenHash;
                            """;

        var parameters = new
        {
            RefreshTokenHash = refreshTokenHash,
        };

        if (await connection.QuerySingleOrDefaultAsync<SessionDao>(sql, parameters) is not { } sessionDao)
        {
            return null;
        }

        return sessionDao.ToDomain();
    }

    public async Task<bool> PromotePendingTokensAsync(
        SessionId sessionId,
        byte[] pendingAccessTokenHash,
        byte[] pendingRefreshTokenHash,
        CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string updateSql = """
                                 UPDATE auth.sessions
                                 SET current_access_token_hash = @PendingAccessTokenHash,
                                     current_refresh_token_hash = @PendingRefreshTokenHash,
                                     pending_access_token_hash = NULL,
                                     pending_refresh_token_hash = NULL
                                 WHERE session_id = @SessionId
                                   AND EXISTS (
                                       SELECT 1
                                       FROM auth.session_pending_tokens pending
                                       WHERE pending.session_id = @SessionId
                                         AND pending.pending_access_token_hash = @PendingAccessTokenHash
                                         AND pending.pending_refresh_token_hash = @PendingRefreshTokenHash
                                   );
                                 """;

        var parameters = new
        {
            SessionId = sessionId.Value,
            PendingAccessTokenHash = pendingAccessTokenHash,
            PendingRefreshTokenHash = pendingRefreshTokenHash,
        };

        var promotedRows = await connection.ExecuteAsync(updateSql, parameters, transaction);

        if (promotedRows == 0)
        {
            transaction.Rollback();
            return false;
        }

        const string deleteSql = """
                                 DELETE FROM auth.session_pending_tokens
                                 WHERE session_id = @SessionId;
                                 """;

        await connection.ExecuteAsync(deleteSql, parameters, transaction);
        transaction.Commit();
        return true;
    }

    public async Task SetPendingTokensAsync(
        SessionId sessionId,
        byte[] pendingAccessTokenHash,
        byte[] pendingRefreshTokenHash,
        CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var parameters = new
        {
            SessionId = sessionId.Value,
            PendingAccessTokenHash = pendingAccessTokenHash,
            PendingRefreshTokenHash = pendingRefreshTokenHash,
        };

        const string insertSql = """
                                 INSERT INTO auth.session_pending_tokens(
                                     session_id,
                                     pending_access_token_hash,
                                     pending_refresh_token_hash)
                                 VALUES (
                                     @SessionId,
                                     @PendingAccessTokenHash,
                                     @PendingRefreshTokenHash);
                                 """;

        await connection.ExecuteAsync(insertSql, parameters, transaction);

        const string updateSql = """
                                 UPDATE auth.sessions
                                 SET pending_access_token_hash = @PendingAccessTokenHash,
                                     pending_refresh_token_hash = @PendingRefreshTokenHash
                                 WHERE session_id = @SessionId;
                                 """;

        await connection.ExecuteAsync(updateSql, parameters, transaction);
        transaction.Commit();
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
                               @PendingRefreshTokenHash);
                           """;

        await connection.ExecuteAsync(sql, new
        {
            SessionId = session.Id.Value,
            UserId = session.UserId.Value,
            CurrentAccessTokenHash = session.AccessTokenHash,
            CurrentRefreshTokenHash = session.RefreshTokenHash,
            session.PendingAccessTokenHash,
            session.PendingRefreshTokenHash,
        });
    }
}