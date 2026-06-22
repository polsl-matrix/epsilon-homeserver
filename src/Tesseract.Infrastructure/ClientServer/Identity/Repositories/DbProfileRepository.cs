using Dapper;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Domain.Users;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.ClientServer.Identity.Repositories;

internal class DbProfileRepository(IDbConnectionFactory dbConnectionFactory) : IProfileRepository
{
    public async Task InsertAsync(Profile profile, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

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

    public async Task UpsertAvatarUrlAsync(UserId userId, string avatarUrl, CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO identity.profiles(user_id, display_name, avatar_url)
                           VALUES (@UserId, NULL, @AvatarUrl)
                           ON CONFLICT (user_id) DO UPDATE SET avatar_url = EXCLUDED.avatar_url;
                           """;

        var parameters = new
        {
            UserId = userId.Value,
            AvatarUrl = avatarUrl,
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<string?> GetDisplayNameAsync(UserId userId, CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           SELECT p.display_name
                           FROM identity.profiles p
                           WHERE p.user_id = @UserId;
                           """;

        var parameters = new
        {
            UserId = userId.Value,
        };

        return await connection.QuerySingleOrDefaultAsync<string?>(sql, parameters);
    }

    public async Task<string?> GetAvatarUrlAsync(UserId userId, CancellationToken cancellationToken)
    {
        await using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           SELECT p.avatar_url
                           FROM identity.profiles p
                           WHERE p.user_id = @UserId;
                           """;

        var parameters = new
        {
            UserId = userId.Value,
        };

        return await connection.QuerySingleOrDefaultAsync<string?>(sql, parameters);
    }
}