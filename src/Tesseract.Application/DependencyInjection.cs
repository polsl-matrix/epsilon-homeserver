using Microsoft.Extensions.DependencyInjection;

namespace Tesseract.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddApplication()
        {
            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            });
        }
    }
}