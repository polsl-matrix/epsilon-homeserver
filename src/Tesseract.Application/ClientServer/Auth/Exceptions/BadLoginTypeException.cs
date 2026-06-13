using ApplicationException = Tesseract.Application.Common.Exceptions.ApplicationException;

namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public sealed class BadLoginTypeException(string type)
    : ApplicationException($"Unrecognized login type: {type}");