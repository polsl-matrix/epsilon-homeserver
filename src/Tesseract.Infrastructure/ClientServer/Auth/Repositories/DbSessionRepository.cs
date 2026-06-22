using Dapper;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;
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
                                   current_refresh_token_hash {nameof(SessionDao.CurrentRefreshTokenHash)}
                            FROM auth.sessions
                            WHERE current_access_token_hash = @AccessTokenHash;
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

    public async Task UpsertAsync(Session session, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO auth.sessions(session_id, user_id, current_access_token_hash, current_refresh_token_hash)
                           VALUES (@SessionId, @UserId, @CurrentAccessTokenHash, @CurrentRefreshTokenHash);
                           """;

        await connection.ExecuteAsync(sql, new
        {
            SessionId = session.Id.Value,
            UserId = session.UserId.Value,
            CurrentAccessTokenHash = session.AccessTokenHash,
            CurrentRefreshTokenHash = session.RefreshTokenHash,
        });
    }

    public async Task DeleteAllByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           DELETE FROM auth.sessions
                           WHERE user_id = @UserId;
                           """;

        await connection.ExecuteAsync(sql, new
        {
            UserId = userId.Value,
        });
    }
}