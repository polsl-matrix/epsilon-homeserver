using Npgsql;
using System.Data;
using Tesseract.Infrastructure.Common.Database.Interfaces;

namespace Tesseract.Infrastructure.Common.Database;

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string? connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() =>
        new NpgsqlConnection(_connectionString);
}