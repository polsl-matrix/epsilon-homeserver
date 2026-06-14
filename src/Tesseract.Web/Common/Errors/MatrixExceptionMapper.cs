using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Web.ClientServer.Auth.Contracts;
using Tesseract.Web.Common.Errors.Contracts;
using Tesseract.Web.Common.Errors.Interfaces;
using static Tesseract.Web.Common.Errors.Contracts.MatrixErrorCodes;

namespace Tesseract.Web.Common.Errors;

using ErrorMapping = (int StatusCode, object ErrorResponse);

internal class MatrixExceptionMapper : IMatrixExceptionMapper
{
    public ErrorMapping Map(Exception exception) => exception switch
    {
        BadLoginTypeException => MapBadLoginType(),
        ForbiddenException => MapForbidden(),
        InvalidUsernameException => MapInvalidUsername(),
        MissingParameterException => MapMissingParameter(),
        RegistrationForbiddenException => MapForbidden(),
        UserInUseException => MapUserInUse(),
        UserInteractiveAuthenticationRequiredException authenticationException => MapUserInteractiveAuthenticationRequired(authenticationException),
        _ => MapUnknown(),
    };

    private static ErrorMapping MapBadLoginType() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(Unknown, "Bad login type."));

    private static ErrorMapping MapForbidden() => (StatusCodes.Status403Forbidden,
        new MatrixErrorResponse(Forbidden));

    private static ErrorMapping MapInvalidUsername() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(InvalidUsername, "The desired user ID is not a valid user name."));

    private static ErrorMapping MapMissingParameter() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(MissingParam, "A required parameter is missing."));

    private static ErrorMapping MapUserInUse() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(UserInUse, "Desired user ID is already taken."));

    private static ErrorMapping MapUserInteractiveAuthenticationRequired(
        UserInteractiveAuthenticationRequiredException exception)
    {
        var flows = exception.Flows
            .Select(flow => new UserInteractiveAuthenticationResponse.FlowInformation
            {
                Stages = flow,
            })
            .ToList();

        return (StatusCodes.Status401Unauthorized, new UserInteractiveAuthenticationResponse
        {
            Completed = exception.Completed.Count == 0 ? null : exception.Completed,
            Flows = flows,
            Params = new Dictionary<string, object>(),
            Session = exception.Session,
            ErrorCode = exception.ErrorCode,
            Error = exception.Error,
        });
    }

    private static ErrorMapping MapUnknown() => (StatusCodes.Status500InternalServerError,
        new MatrixErrorResponse(Unknown, "An unknown error has occurred."));
}