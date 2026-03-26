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

    extension(IServiceCollection services)
    {
        public void AddWeb()
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(DefaultCorsPolicy);
            });

            services.AddControllers();

            services.AddOpenApi();
        }
    }
}