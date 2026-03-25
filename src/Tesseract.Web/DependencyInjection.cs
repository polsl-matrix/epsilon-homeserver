using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Tesseract.Web;

public static class DependencyInjection
{
    private static void DefaultCorsPolicy(CorsPolicyBuilder policy)
    {
        policy.AllowAnyOrigin();
        policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
        policy.WithHeaders("X-Requested-With", "Content-Type", "Authorization");
    }

    extension(IHostApplicationBuilder builder)
    {
        public void AddWebServices()
        {
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(DefaultCorsPolicy);
            });

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();
        }
    }
}