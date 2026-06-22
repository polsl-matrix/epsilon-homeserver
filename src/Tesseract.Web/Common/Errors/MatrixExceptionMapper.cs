using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Exceptions;
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
        ForbiddenException => MapForbidden(),
        InvalidUsernameException => MapInvalidUsername(),
        ProfileFieldNotFoundException => MapProfileFieldNotFound(),
        CannotUpdateOtherUserProfileException => MapProfileUpdateForbidden(),
        UsernameTakenException => MapUsernameTaken(),
        RoomNotFoundException => MapRoomNotFound(),
        UserNotFoundException => MapUserNotFound(),
        UserNotInRoomException => MapUserNotInRoom(),
        _ => MapUnknown(),
    };

    private static ErrorMapping MapBadLoginType() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(Unknown, "Bad login type."));

    private static ErrorMapping MapForbidden() => (StatusCodes.Status403Forbidden,
        new MatrixErrorResponse(Forbidden, "Forbidden."));

    private static ErrorMapping MapInvalidUsername() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(InvalidUsername, "Provided username is not valid."));

    private static ErrorMapping MapUsernameTaken() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(UserInUse, "Provided username is already taken."));

    private static ErrorMapping MapProfileFieldNotFound() => (StatusCodes.Status404NotFound,
        new MatrixErrorResponse(NotFound, "Profile field was not found."));

    private static ErrorMapping MapProfileUpdateForbidden() => (StatusCodes.Status403Forbidden,
        new MatrixErrorResponse(Forbidden, "Cannot update another user's profile."));

    private static ErrorMapping MapUnknown() => (StatusCodes.Status500InternalServerError,
        new MatrixErrorResponse(Unknown, "An unknown error has occurred."));

    private static ErrorMapping MapRoomNotFound() => (StatusCodes.Status404NotFound,
        new MatrixErrorResponse(NotFound, "Room not found."));

    private static ErrorMapping MapUserNotFound() => (StatusCodes.Status404NotFound,
        new MatrixErrorResponse(NotFound, "User not found."));

    private static ErrorMapping MapUserNotInRoom() => (StatusCodes.Status400BadRequest,
        new MatrixErrorResponse(Forbidden, "User does not participate in the room."));
}