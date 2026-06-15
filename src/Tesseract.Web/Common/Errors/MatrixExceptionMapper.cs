using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Web.Common.Errors.Contracts;
using Tesseract.Web.Common.Errors.Interfaces;
using static Tesseract.Web.Common.Errors.Contracts.MatrixErrorCodes;

namespace Tesseract.Web.Common.Errors;

using ErrorMapping = (int StatusCode, MatrixErrorResponse ErrorResponse);

internal class MatrixExceptionMapper : IMatrixExceptionMapper
{
    public ErrorMapping Map(Exception exception) => exception switch
    {
        BadLoginTypeException => MapBadLoginType(),
        ForbiddenException => MapForbidden(),
        InvalidUsernameException => MapInvalidUsername(),
        UnknownTokenException => MapUnknownToken(),
        UsernameTakenException => MapUsernameTaken(),
        _ => MapUnknown(),
    };

    private static ErrorMapping MapBadLoginType() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(Unknown, "Bad login type."));

    private static ErrorMapping MapForbidden() => (StatusCodes.Status403Forbidden,
        new MatrixErrorResponse(Forbidden));

    private static ErrorMapping MapInvalidUsername() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(InvalidUsername, "Provided username is not valid."));

    private static ErrorMapping MapUnknownToken() => (StatusCodes.Status401Unauthorized,
        new MatrixErrorResponse(UnknownToken));

    private static ErrorMapping MapUsernameTaken() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(UserInUse, "Provided username is already taken."));

    private static ErrorMapping MapUnknown() => (StatusCodes.Status500InternalServerError,
        new MatrixErrorResponse(Unknown, "An unknown error has occurred."));
}