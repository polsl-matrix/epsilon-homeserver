using ApplicationException = Tesseract.Application.Common.Exceptions.ApplicationException;

namespace Tesseract.Application.ClientServer.Auth.Exceptions;

public sealed class ForbiddenException
    : ApplicationException;