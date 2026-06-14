namespace Tesseract.Application.ClientServer.Auth.Models;

public sealed class LoginFlow(string type)
{
    public string Type { get; } = type;
}