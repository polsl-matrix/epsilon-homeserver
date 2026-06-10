using Tesseract.Application.ClientServer.Registration.Exceptions;
using Tesseract.Web.Common.Errors.Contracts;
using static Tesseract.Web.Common.Errors.Contracts.MatrixErrorCodes;

namespace Tesseract.Web.Common.Errors.Interfaces;

using ErrorMapping = (int StatusCode, MatrixErrorResponse ErrorResponse);

internal class MatrixExceptionMapper : IMatrixExceptionMapper
{
    public ErrorMapping Map(Exception exception) => exception switch
    {
        InvalidUsernameException invalidUsername => (StatusCodes.Status400BadRequest,
            new MatrixErrorResponse(InvalidUsername, invalidUsername.Message)),

        UserInUseException userInUse => (StatusCodes.Status400BadRequest,
            new MatrixErrorResponse(UserInUse, userInUse.Message)),

        RegistrationNotAllowedException registrationNotAllowed => (StatusCodes.Status403Forbidden,
            new MatrixErrorResponse(Forbidden, registrationNotAllowed.Message)),

        _ => MapUnknown(),
    };

    private static ErrorMapping MapUnknown() => (StatusCodes.Status500InternalServerError,
        new MatrixErrorResponse(Unknown, "An unknown error has occurred."));
}