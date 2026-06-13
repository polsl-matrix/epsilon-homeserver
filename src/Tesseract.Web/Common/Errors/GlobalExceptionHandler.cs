using Microsoft.AspNetCore.Diagnostics;
using Tesseract.Web.Common.Errors.Interfaces;

namespace Tesseract.Web.Common.Errors;

internal class GlobalExceptionHandler(IMatrixExceptionMapper exceptionMapper) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, errorResponse) = exceptionMapper.Map(exception);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }
}