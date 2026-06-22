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

    public async Task UpsertDisplayNameAsync(
        UserHandle handle, string? displayName, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO identity.profiles(user_id, display_name, avatar_url)
                           SELECT user_id, @DisplayName, NULL
                           FROM identity.users
                           WHERE localpart = @Localpart AND domain = @Domain
                           ON CONFLICT (user_id)
                           DO UPDATE SET display_name = EXCLUDED.display_name;
                           """;

        var parameters = new
        {
            Localpart = handle.Localpart.Value,
            Domain = handle.Domain.Value,
            DisplayName = displayName,
        };

        await connection.ExecuteAsync(sql, parameters);
    }
}