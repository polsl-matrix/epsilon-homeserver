using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        }
    }
}