namespace Tesseract.Application.ClientServer.Registration.Abstractions;

public interface IIdentifierGenerator
{
    string GenerateLocalpart();

    string GenerateDeviceId();

    string GenerateAccessToken();
}