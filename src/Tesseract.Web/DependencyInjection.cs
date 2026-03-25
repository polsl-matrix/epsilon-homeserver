using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Tesseract.Web;

public static class DependencyInjection
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddWebServices()
        {
            builder.Services.AddControllers();

            builder.Services.AddOpenApi();
        }
    }
}