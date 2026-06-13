using Dapper;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;
using Tesseract.Infrastructure.ClientServer.Identity.Dao;
using Tesseract.Infrastructure.ClientServer.Identity.Mappers;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Identity.Repositories;

internal class DbUserRepository(IDbConnectionFactory dbConnectionFactory) : IUserRepository
{
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                            SELECT user_id {nameof(UserDao.UserId)},
                                   localpart {nameof(UserDao.Localpart)},
                                   domain {nameof(UserDao.Domain)}
                            FROM identity.users
                            WHERE user_id = @UserId;
                            """;

        var parameters = new
        {
            UserId = id.Value,
        };

        if (await connection.QuerySingleOrDefaultAsync<UserDao>(sql, parameters) is not { } userDao)
        {
            return null;
        }

        return userDao.ToDomain();
    }

    public async Task<User?> GetByHandleAsync(UserHandle handle, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                            SELECT user_id {nameof(UserDao.UserId)},
                                   localpart {nameof(UserDao.Localpart)},
                                   domain {nameof(UserDao.Domain)}
                            FROM identity.users
                            WHERE localpart = @Localpart
                              AND domain = @Domain;
                            """;

        var parameters = new
        {
            handle.Localpart,
            handle.Domain,
        };

        if (await connection.QuerySingleOrDefaultAsync<UserDao>(sql, parameters) is not { } userDao)
        {
            return null;
        }

        return userDao.ToDomain();
    }
}