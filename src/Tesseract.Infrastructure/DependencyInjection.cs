using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tesseract.Application;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Infrastructure.ClientServer.Discovery;
using Tesseract.Infrastructure.Common.Database;
using Tesseract.Infrastructure.Common.Database.Abstractions;

namespace Tesseract.Infrastructure;

public static class DependencyInjection
{
    private static string? GetConnectionString(this IServiceProvider provider, string name)
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        return configuration.GetConnectionString(name);
    }

    extension(IServiceCollection services)
    {
        public void AddInfrastructure()
        {
            services.AddSingleton<IDbConnectionFactory>(provider =>
                new NpgsqlConnectionFactory(provider.GetConnectionString("App")));

            services.AddScoped<IVersionRepository, VersionRepository>();
            services.AddScoped<IRandomRepo, RandomRepo>();
        }
    }
}