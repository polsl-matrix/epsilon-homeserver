using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Application.ClientServer.Registration.Abstractions;
using Tesseract.Infrastructure.ClientServer.Discovery;
using Tesseract.Infrastructure.ClientServer.Registration;
using Tesseract.Infrastructure.Common.Database;
using Tesseract.Infrastructure.Common.Database.Abstractions;
using Tesseract.Infrastructure.Common.Security;

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

            builder.Services.Configure<MatrixOptions>(
                builder.Configuration.GetSection("Matrix"));

            builder.Services.AddScoped<IVersionRepository, VersionRepository>();
            builder.Services.AddScoped<IWellKnownRepository, WellKnownRepository>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();

            builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            builder.Services.AddSingleton<IIdentifierGenerator, SecureIdentifierGenerator>();
        }
    }
}