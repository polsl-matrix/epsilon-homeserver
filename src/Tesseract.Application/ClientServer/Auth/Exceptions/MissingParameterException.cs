namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public sealed class MissingParameterException(string parameterName)
    : Exception($"Missing required parameter: {parameterName}.")
{
    public string ParameterName { get; } = parameterName;
}