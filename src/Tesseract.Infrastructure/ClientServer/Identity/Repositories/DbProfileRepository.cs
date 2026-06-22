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
}