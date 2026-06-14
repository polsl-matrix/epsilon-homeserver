using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Web.ClientServer.Auth.Contracts;
using Tesseract.Web.Common.Errors;
using Tesseract.Web.Common.Errors.Contracts;

namespace Tesseract.Web.Tests.Common.Errors;

public class MatrixExceptionMapperTests
{
    private readonly MatrixExceptionMapper _exceptionMapper = new();

    public static TheoryData<Exception, int, string, string?> KnownExceptions => new()
    {
        // @formatter:off
        { new BadLoginTypeException("m.test.unknown"), StatusCodes.Status400BadRequest, MatrixErrorCodes.Unknown, "bad login" },
        { new ForbiddenException(), StatusCodes.Status403Forbidden, MatrixErrorCodes.Forbidden, null },
        { new InvalidUsernameException("BadUser"), StatusCodes.Status400BadRequest, MatrixErrorCodes.InvalidUsername, "valid user name" },
        { new MissingParameterException("password"), StatusCodes.Status400BadRequest, MatrixErrorCodes.MissingParam, "required parameter" },
        { new RegistrationForbiddenException("Registration is disabled"), StatusCodes.Status403Forbidden, MatrixErrorCodes.Forbidden, null },
        { new UserInUseException("@alice:example.com"), StatusCodes.Status400BadRequest, MatrixErrorCodes.UserInUse, "already taken" },
        // @formatter:on
    };

    [Fact]
    public void Map_UnknownException_ReturnsValidResponse()
    {
        var exception = new UnknownException();

        var (httpStatus, response) = _exceptionMapper.Map(exception);

        httpStatus.Should().Be(StatusCodes.Status500InternalServerError);
        var errorResponse = response.Should().BeOfType<MatrixErrorResponse>().Subject;
        errorResponse.Code.Should().Be(MatrixErrorCodes.Unknown);
        errorResponse.Message.Should().ContainEquivalentOf("unknown error");
    }

    [Theory]
    [MemberData(nameof(KnownExceptions))]
    public void Map_KnownException_ReturnsValidResponse(Exception exception, int status, string code, string? message)
    {
        var (httpStatus, response) = _exceptionMapper.Map(exception);

        httpStatus.Should().Be(status);
        var errorResponse = response.Should().BeOfType<MatrixErrorResponse>().Subject;
        errorResponse.Code.Should().Be(code);

        if (message is not null)
        {
            errorResponse.Message.Should().ContainEquivalentOf(message);
        }
    }

    [Fact]
    public void Map_UserInteractiveAuthenticationRequiredException_ReturnsAuthenticationResponse()
    {
        var exception = new UserInteractiveAuthenticationRequiredException(
            [["m.login.dummy"]],
            "session-id",
            errorCode: MatrixErrorCodes.Forbidden,
            error: "Unsupported authentication type.");

        var (httpStatus, response) = _exceptionMapper.Map(exception);

        httpStatus.Should().Be(StatusCodes.Status401Unauthorized);
        var authenticationResponse = response.Should().BeOfType<UserInteractiveAuthenticationResponse>().Subject;
        authenticationResponse.Flows.Should().ContainSingle()
            .Which.Stages.Should().ContainSingle().Which.Should().Be("m.login.dummy");
        authenticationResponse.Params.Should().BeEmpty();
        authenticationResponse.Session.Should().Be("session-id");
        authenticationResponse.ErrorCode.Should().Be(MatrixErrorCodes.Forbidden);
        authenticationResponse.Error.Should().Contain("Unsupported");
    }

    private class UnknownException : Exception;
}