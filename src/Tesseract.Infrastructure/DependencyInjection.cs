using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Common.Repositories;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Rooms.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Domain.Rooms;
using Tesseract.Infrastructure.ClientServer.Auth.Repositories;
using Tesseract.Infrastructure.ClientServer.Auth.Services;
using Tesseract.Infrastructure.ClientServer.Common;
using Tesseract.Infrastructure.ClientServer.Discovery;
using Tesseract.Infrastructure.ClientServer.Discovery.Configuration;
using Tesseract.Infrastructure.ClientServer.Events;
using Tesseract.Infrastructure.ClientServer.Events.Mappers;
using Tesseract.Infrastructure.ClientServer.Identity.Repositories;
using Tesseract.Infrastructure.ClientServer.Rooms;
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
            var services = builder.Services;

            var connectionString = builder.Configuration
                .GetConnectionString("Default");

            services.AddSingleton<IDbConnectionFactory>(_ =>
                new NpgsqlConnectionFactory(connectionString));

            services.AddOptions<MatrixConfigurationOptions>()
                .BindConfiguration(MatrixConfigurationOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<DiscoveryOptions>()
                .BindConfiguration(DiscoveryOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddScoped<IAccessTokenService, OpaqueTokenService>();
            services.AddScoped<IHashService, Sha256Hasher>();
            services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
            services.AddScoped<IRefreshTokenService, OpaqueTokenService>();

            services.AddScoped<IEventRepository, DbEventRepository>();
            services.AddScoped<IEventMapper, CreateRoomEventMapper>();
            services.AddScoped<IEventMapper, MemberRoomEventMapper>();
            services.AddScoped<IEventMapper, MessageRoomEventMapper>();

            services.AddScoped<IMatrixConfigurationRepository, MatrixConfigurationRepository>();
            services.AddScoped<IPasswordRepository, DbPasswordRepository>();
            services.AddScoped<IProfileRepository, DbProfileRepository>();
            services.AddScoped<IResponseRepository, DbResponseRepository>();
            services.AddScoped<IRoomRepository, DbRoomRepository>();
            services.AddScoped<IRoomMembershipRepository, DbRoomMembershipRepository>();
            services.AddScoped<IRoomMessageRepository, DbRoomMessageRepository>();
            services.AddScoped<ISessionRepository, DbSessionRepository>();
            services.AddScoped<IUserRepository, DbUserRepository>();
            services.AddScoped<IVersionRepository, InMemoryVersionRepository>();
            services.AddScoped<IWellKnownRepository, WellKnownRepository>();
        }
    }
}