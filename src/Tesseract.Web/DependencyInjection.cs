using Microsoft.AspNetCore.Cors.Infrastructure;
using Tesseract.Infrastructure.ClientServer.Auth.Extensions;
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

    extension(IHostApplicationBuilder builder)
    {
        public void AddWeb()
        {
            var services = builder.Services;

            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(DefaultCorsPolicy);
            });

            services.AddSingleton<IMatrixExceptionMapper, MatrixExceptionMapper>();

            services.AddControllers();
            services.AddOpenApi();
        }

        public void AddAuth()
        {
            var services = builder.Services;

            const string opaqueTokenAuthScheme = "opaque-token";

            services.AddAuthentication(opaqueTokenAuthScheme)
                .AddOpaqueToken(opaqueTokenAuthScheme);
            services.AddAuthorization();
        }
    }
}