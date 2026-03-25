using Tesseract.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddWebServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();

app.Run();