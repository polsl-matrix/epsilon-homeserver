using Dapper;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;
using Tesseract.Infrastructure.ClientServer.Auth.Dao;
using Tesseract.Infrastructure.ClientServer.Auth.Mappers;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Auth.Repositories;

public class DbPasswordRepository(IDbConnectionFactory dbConnectionFactory) : IPasswordRepository
{
    public async Task InsertAsync(Password password, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO auth.passwords(user_id, password_hash)
                           VALUES (@UserId, @PasswordHash);
                           """;

        var parameters = new
        {
            UserId = password.UserId.Value,
            PasswordHash = password.Hash,
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<Password?> GetAsync(UserId userId, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                            SELECT user_id       {nameof(PasswordDao.UserId)},
                                   password_hash {nameof(PasswordDao.PasswordHash)}
                            FROM auth.passwords
                            WHERE user_id = @UserId;
                            """;

        var parameters = new
        {
            UserId = userId.Value,
        };

        if (await connection.QuerySingleOrDefaultAsync<PasswordDao>(sql, parameters) is not { } passwordDao)
        {
            return null;
        }

        return passwordDao.ToDomain();
    }
}