using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Infrastructure.ClientServer.Auth;
using Tesseract.Infrastructure.ClientServer.Discovery;
using Tesseract.Infrastructure.Common.Database;
using Tesseract.Infrastructure.Common.Database.Abstractions;

namespace Tesseract.Infrastructure;

public static class DependencyInjection
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddInfrastructure()
        {
            var connectionString = builder.Configuration
                .GetConnectionString("Default");

            builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
                new NpgsqlConnectionFactory(connectionString));

            builder.Services.AddScoped<IAccessTokenService, OpaqueTokenService>();
            builder.Services.AddScoped<IRefreshTokenService, OpaqueTokenService>();
            builder.Services.AddScoped<IHashService, Sha256HashService>();

            builder.Services.AddScoped<IVersionRepository, VersionRepository>();
            builder.Services.AddScoped<ISessionRepository, DbSessionRepository>();
        }
    }
}