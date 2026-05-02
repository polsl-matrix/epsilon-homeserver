using Microsoft.AspNetCore.Cors.Infrastructure;
using Tesseract.Web.Common.Errors;
using Tesseract.Web.Common.Errors.Interfaces;

namespace Tesseract.Web;

public static class DependencyInjection
{
    private static void DefaultCorsPolicy(CorsPolicyBuilder policy)
    {
        policy.AllowAnyOrigin();
        policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
        policy.WithHeaders("X-Requested-With", "Content-Type", "Authorization");
    }

    private static void AddServices(IServiceCollection services) =>
        services.AddSingleton<IMatrixExceptionMapper, MatrixExceptionMapper>();

    extension(IServiceCollection services)
    {
        public void AddWeb()
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(DefaultCorsPolicy);
            });

            AddServices(services);

            services.AddControllers();

            services.AddOpenApi();
        }
    }
}