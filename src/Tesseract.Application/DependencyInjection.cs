using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Flows;
using Tesseract.Application.ClientServer.Auth.Implementations;
using Tesseract.Application.Common.Transactions;

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
                options.AddOpenBehavior(typeof(TransactionBehavior<,>));
            });

            services.AddScoped<IAuthenticationFlow, DummyAuthenticationFlow>();
            services.AddScoped<IAuthenticationFlow, PasswordAuthenticationFlow>();
            services.AddScoped<ISessionFactory, SessionFactory>();
        }
    }
}