using Tesseract.Application;
using Tesseract.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddWeb();

var app = builder.Build();

app.UseExceptionHandler(_ => { });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();

app.Run();