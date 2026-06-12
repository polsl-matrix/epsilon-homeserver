using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Flows;

namespace Tesseract.Application;

public static class DependencyInjection
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplication()
        {
            var services = builder.Services;

            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            });

            services.AddScoped<IAuthenticationFlow, DummyAuthenticationFlow>();
            services.AddScoped<ISessionFactory, SessionFactory>();
        }
    }
}