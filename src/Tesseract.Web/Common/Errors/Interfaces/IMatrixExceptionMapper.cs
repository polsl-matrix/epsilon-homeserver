namespace Tesseract.Web.Common.Errors.Interfaces;

using ErrorMapping = (int StatusCode, object ErrorResponse);

public interface IMatrixExceptionMapper
{
    ErrorMapping Map(Exception exception);
}