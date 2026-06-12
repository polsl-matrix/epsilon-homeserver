using Serilog;
using Tesseract.Application;
using Tesseract.Infrastructure;
using Tesseract.Infrastructure.Common.Database;
using Tesseract.Web;
using Tesseract.Web.Common.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.AddApplication();
builder.AddInfrastructure();
builder.AddWeb();

builder.AddOpenTelemetry();

var app = builder.Build();

app.RunMigrations();

app.UseExceptionHandler(_ => { });

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.Run();