using Dapper;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;
using Tesseract.Infrastructure.ClientServer.Auth.Dao;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Auth;

public class DbUserRepository(IDbConnectionFactory dbConnectionFactory) : IUserRepository
{
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = $"""
                            SELECT user_id {nameof(UserDao.UserId)},
                                   localpart {nameof(UserDao.Localpart)},
                                   domain {nameof(UserDao.Domain)}
                            FROM identity.users;
                            """;

        if (await connection.QuerySingleOrDefaultAsync<UserDao>(sql) is not { } dao)
        {
            return null;
        }

        var userId = new UserId(dao.UserId);
        var handle = new Handle(dao.Localpart, dao.Domain);

        return new User(userId, handle);
    }
}