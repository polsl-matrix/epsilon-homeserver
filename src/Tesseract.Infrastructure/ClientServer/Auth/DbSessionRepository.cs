using Dapper;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Infrastructure.Common.Database;

namespace Tesseract.Infrastructure.ClientServer.Auth;

public class DbSessionRepository(IDbConnectionFactory dbConnectionFactory) : ISessionRepository
{
    public async Task UpsertSessionAsync(Session session, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO auth.sessions(session_id, user_id, current_auth_token_hash, current_refresh_token_hash)
                           VALUES (@SessionId, @UserId, @CurrentAuthTokenHash, @CurrentRefreshTokenHash);
                           """;

        await connection.ExecuteAsync(sql, new
        {
            SessionId = session.Id.Value,
            UserId = session.UserId.Value,
            CurrentAuthTokenHash = session.AccessTokenHash,
            CurrentRefreshTokenHash = session.RefreshTokenHash,
        });
    }
}