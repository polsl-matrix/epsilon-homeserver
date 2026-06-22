using Dapper;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Identity.Repositories;

internal class DbProfileRepository(IDbConnectionFactory dbConnectionFactory) : IProfileRepository
{
    public async Task InsertAsync(Profile profile, CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO identity.profiles(user_id, display_name, avatar_url)
                           VALUES (@UserId, @DisplayName, @AvatarUrl);
                           """;

        var parameters = new
        {
            UserId = profile.UserId.Value,
            profile.DisplayName,
            profile.AvatarUrl,
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task UpsertDisplayNameAsync(UserId userId, string displayName, CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO identity.profiles(user_id, display_name, avatar_url)
                           VALUES (@UserId, @DisplayName, NULL)
                           ON CONFLICT (user_id) DO UPDATE SET display_name = EXCLUDED.display_name;
                           """;

        var parameters = new
        {
            UserId = userId.Value,
            DisplayName = displayName,
        };

        await connection.ExecuteAsync(sql, parameters);
    }
}