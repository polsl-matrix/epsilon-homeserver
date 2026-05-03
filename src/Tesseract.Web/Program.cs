using Serilog;
using Tesseract.Application;
using Tesseract.Infrastructure;
using Tesseract.Infrastructure.Common.Database;
using Tesseract.Web;
using Tesseract.Web.Common.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.AddApplication();
builder.AddInfrastructure();
builder.Services.AddWeb();
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

app.Run();