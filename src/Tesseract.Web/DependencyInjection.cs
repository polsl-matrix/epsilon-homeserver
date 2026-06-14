using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
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

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("auth", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 20;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });
            });

            services.AddSingleton<IMatrixExceptionMapper, MatrixExceptionMapper>();

            services.AddControllers();
            services.AddOpenApi();
        }

        public void AddAuth()
        {
            var services = builder.Services;

            services.AddAuthentication("AccessToken")
                .AddOpaqueToken("AccessToken");
            services.AddAuthorization();
        }
    }
}