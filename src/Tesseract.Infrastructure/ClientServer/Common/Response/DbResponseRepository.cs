using Dapper;
using Tesseract.Application.ClientServer.Common.Repositories;
using Tesseract.Domain.Users;
using Tesseract.Infrastructure.ClientServer.Common.Dao;
using Tesseract.Infrastructure.ClientServer.Common.Mappers;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Common;

internal class DbResponseRepository(IDbConnectionFactory dbConnectionFactory) : IResponseRepository
{
    public async Task UpsertAsync(Response response, CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO misc.responses(user_id, path_hash, status_code, content_type, content)
                           VALUES (@UserId, MD5(@Path)::BYTEA, @StatusCode, @ContentType, @Content)
                           ON CONFLICT(user_id, path_hash)
                               DO UPDATE SET status_code  = EXCLUDED.status_code,
                                             content_type = EXCLUDED.content_type,
                                             content      = EXCLUDED.content;
                           """;

        var parameters = new
        {
            UserId = response.UserId.Value,
            Path = response.Path,
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            Content = response.Content,
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<Response?> GetByUserIdAndPathAsync(UserId userId, string path,
        CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                            SELECT user_id      {nameof(ResponseDao.UserId)},
                                   status_code  {nameof(ResponseDao.StatusCode)},
                                   content_type {nameof(ResponseDao.ContentType)},
                                   content      {nameof(ResponseDao.Content)}
                            FROM misc.responses
                            WHERE user_id = @UserId
                              AND MD5(@Path)::BYTEA = path_hash;
                            """;

        var parameters = new
        {
            UserId = userId.Value,
            Path = path,
        };

        if (await connection.QuerySingleOrDefaultAsync<ResponseDao>(sql, parameters) is not { } responseDao)
        {
            return null;
        }

        return responseDao.ToDomain(path);
    }
}