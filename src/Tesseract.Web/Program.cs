using Tesseract.Application;
using Tesseract.Web;
using Tesseract.Web.Common.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddWeb();
builder.AddOpenTelemetry();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();

app.Run();