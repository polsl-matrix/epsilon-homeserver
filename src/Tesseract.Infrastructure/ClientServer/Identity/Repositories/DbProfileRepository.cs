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

    public async Task<string?> GetDisplayNameAsync(UserHandle handle, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        const string sql = """
                           SELECT p.display_name
                           FROM identity.profiles p
                           INNER JOIN identity.users u ON u.user_id = p.user_id
                           WHERE u.localpart = @Localpart
                             AND u.domain = @Domain;
                           """;

        var parameters = new
        {
            Localpart = handle.Localpart.Value,
            Domain = handle.Domain.Value,
        };

        return await connection.QuerySingleOrDefaultAsync<string?>(sql, parameters);
    }
}