namespace Tesseract.Domain.Common.Exceptions;

public class ValidationException(string message) : DomainException(message);