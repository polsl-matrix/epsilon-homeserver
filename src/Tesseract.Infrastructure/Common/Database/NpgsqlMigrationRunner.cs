using DbUp;
using DbUp.Engine.Output;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tesseract.Infrastructure.Common.Database;

public static class NpgsqlMigrationRunner
{
    extension(IHost host)
    {
        public void RunMigrations()
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var configuration = services.GetRequiredService<IConfiguration>();
            var appLogger = services.GetRequiredService<ILogger<NpgsqlConnectionFactory>>();

            var connectionString = configuration.GetConnectionString("Default");
            var upgradeLogger = new MicrosoftUpgradeLog(appLogger);

            EnsureDatabase.For.PostgresqlDatabase(connectionString, upgradeLogger);

            var currentAssembly = typeof(NpgsqlMigrationRunner).Assembly;
            var currentNamespace = typeof(NpgsqlMigrationRunner).Namespace;

            var upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(currentAssembly, path => path.Contains(currentNamespace!))
                .LogTo(upgradeLogger)
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw result.Error;
            }
        }
    }
}