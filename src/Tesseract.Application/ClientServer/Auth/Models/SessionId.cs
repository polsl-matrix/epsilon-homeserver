namespace Tesseract.Application.ClientServer.Auth.Models;

public readonly record struct SessionId(Guid Value)
{
    public static SessionId Random() => new(Guid.NewGuid());
}