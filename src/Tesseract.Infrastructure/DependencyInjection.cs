using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Infrastructure.ClientServer.Auth.Repositories;
using Tesseract.Infrastructure.ClientServer.Auth.Services;
using Tesseract.Infrastructure.ClientServer.Discovery;
using Tesseract.Infrastructure.ClientServer.Identity.Repositories;
using Tesseract.Infrastructure.Common.Configuration;
using Tesseract.Infrastructure.Common.Database;
using Tesseract.Infrastructure.Common.Database.Interfaces;

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

            builder.Services.AddOptions<MatrixConfigurationOptions>()
                .BindConfiguration(MatrixConfigurationOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddScoped<IAccessTokenService, OpaqueTokenService>();
            builder.Services.AddScoped<IHashService, Sha256HashService>();
            builder.Services.AddScoped<IRefreshTokenService, OpaqueTokenService>();

            builder.Services.AddScoped<IMatrixConfigurationRepository, MatrixConfigurationRepository>();
            builder.Services.AddScoped<ISessionRepository, DbSessionRepository>();
            builder.Services.AddScoped<IUserRepository, DbUserRepository>();
            builder.Services.AddScoped<IVersionRepository, InMemoryVersionRepository>();
        }
    }
}