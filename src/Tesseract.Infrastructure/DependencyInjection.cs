using Microsoft.Extensions.DependencyInjection;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Infrastructure.ClientServer.Discovery;

namespace Tesseract.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddInfrastructure()
        {
            services.AddScoped<IVersionRepository, VersionRepository>();
        }
    }
}