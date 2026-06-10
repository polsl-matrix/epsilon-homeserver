namespace Tesseract.Application.ClientServer.Registration.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
}