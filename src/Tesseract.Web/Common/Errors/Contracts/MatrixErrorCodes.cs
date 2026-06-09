namespace Tesseract.Web.Common.Errors.Contracts;

public static class MatrixErrorCodes
{
    public const string Forbidden = "M_FORBIDDEN";
    public const string UnknownToken = "M_UNKNOWN_TOKEN";
    public const string MissingToken = "M_MISSING_TOKEN";
    public const string BadJson = "M_BAD_JSON";
    public const string NotJson = "M_NOT_JSON";
    public const string NotFound = "M_NOT_FOUND";
    public const string LimitExceeded = "M_LIMIT_EXCEEDED";
    public const string Unrecognized = "M_UNRECOGNIZED";
    public const string Unknown = "M_UNKNOWN";

    public const string Unauthorized = "M_UNAUTHORIZED";
    public const string UserDeactivated = "M_USER_DEACTIVATED";
    public const string UserInUse = "M_USER_IN_USE";
    public const string InvalidUsername = "M_INVALID_USERNAME";
    public const string RoomInUse = "M_ROOM_IN_USE";
    public const string InvalidRoomState = "M_INVALID_ROOM_STATE";
    public const string ThreePidInUse = "M_THREEPID_IN_USE";
    public const string ThreePidNotFound = "M_THREEPID_NOT_FOUND";
    public const string ThreePidAuthFailed = "M_THREEPID_AUTH_FAILED";
    public const string ThreePidDenied = "M_THREEPID_DENIED";
    public const string ServerNotTrusted = "M_SERVER_NOT_TRUSTED";
    public const string UnsupportedRoomVersion = "M_UNSUPPORTED_ROOM_VERSION";
    public const string IncompatibleRoomVersion = "M_INCOMPATIBLE_ROOM_VERSION";
    public const string BadState = "M_BAD_STATE";
    public const string GuestAccessForbidden = "M_GUEST_ACCESS_FORBIDDEN";
    public const string CaptchaNeeded = "M_CAPTCHA_NEEDED";
    public const string CaptchaInvalid = "M_CAPTCHA_INVALID";
    public const string MissingParam = "M_MISSING_PARAM";
    public const string InvalidParam = "M_INVALID_PARAM";
    public const string TooLarge = "M_TOO_LARGE";
    public const string Exclusive = "M_EXCLUSIVE";
    public const string ResourceLimitExceeded = "M_RESOURCE_LIMIT_EXCEEDED";
    public const string CannotLeaveServerNoticeRoom = "M_CANNOT_LEAVE_SERVER_NOTICE_ROOM";
}