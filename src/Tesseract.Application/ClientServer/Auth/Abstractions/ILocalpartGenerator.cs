using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.ClientServer.Auth.Abstractions;

public interface ILocalpartGenerator
{
    Localpart Create();
}