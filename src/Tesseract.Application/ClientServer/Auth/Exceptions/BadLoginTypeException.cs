using ApplicationException = Tesseract.Application.Common.Exceptions.ApplicationException;

namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public class BadLoginTypeException(string type)
    : ApplicationException($"Unrecognized login type: {type}");