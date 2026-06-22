using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Application.ClientServer.Rooms.Exceptions;
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
        UnauthorizedAccessException => MapUnauthorized(),
        ForbiddenException => MapForbidden(),
        InvalidUsernameException => MapInvalidUsername(),
        CannotUpdateOtherUserProfileException => MapProfileUpdateForbidden(),
        UsernameTakenException => MapUsernameTaken(),
        RoomNotFoundException => MapRoomNotFound(),
        UserNotInRoomException => MapUserNotInRoomException(),
        _ => MapUnknown(),
    };

    private static ErrorMapping MapUnauthorized() => (StatusCodes.Status401Unauthorized,
        new MatrixErrorResponse(Unauthorized, "Unauthorized"));

    private static ErrorMapping MapBadLoginType() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(Unknown, "Bad login type."));

    private static ErrorMapping MapForbidden() => (StatusCodes.Status403Forbidden,
        new MatrixErrorResponse(Forbidden, "Forbidden."));

    private static ErrorMapping MapInvalidUsername() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(InvalidUsername, "Provided username is not valid."));

    private static ErrorMapping MapUsernameTaken() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(UserInUse, "Provided username is already taken."));

    private static ErrorMapping MapProfileUpdateForbidden() => (StatusCodes.Status403Forbidden,
        new MatrixErrorResponse(Forbidden, "Cannot update another user's profile."));

    private static ErrorMapping MapUnknown() => (StatusCodes.Status500InternalServerError,
        new MatrixErrorResponse(Unknown, "An unknown error has occurred."));

    private static ErrorMapping MapRoomNotFound() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(NotFound, "Room not found."));

    private static ErrorMapping MapUserNotInRoomException() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(Forbidden, "User does not participate in the room."));
}