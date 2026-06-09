using Tesseract.Web.Common.Errors.Contracts;

namespace Tesseract.Web.Common.Errors;

using ErrorMapping = (int StatusCode, MatrixErrorResponse ErrorResponse);

public interface IMatrixExceptionMapper
{
    ErrorMapping Map(Exception exception);
}