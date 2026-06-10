using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tesseract.Application.ClientServer.Registration.Exceptions;
using Tesseract.Web.Common.Errors.Contracts;
using Tesseract.Web.Common.Errors.Interfaces;

namespace Tesseract.Web.Tests.Common.Errors;

public class MatrixExceptionMapperTests
{
    private readonly MatrixExceptionMapper _exceptionMapper = new();

    [Fact]
    public void Map_InvalidUsernameException_ReturnsBadRequestWithInvalidUsernameCode()
    {
        var exception = new InvalidUsernameException("The desired user ID is not a valid user name.");

        var (status, response) = _exceptionMapper.Map(exception);

        status.Should().Be(StatusCodes.Status400BadRequest);
        response.Code.Should().Be(MatrixErrorCodes.InvalidUsername);
        response.Message.Should().Be(exception.Message);
    }

    [Fact]
    public void Map_UserInUseException_ReturnsBadRequestWithUserInUseCode()
    {
        var exception = new UserInUseException("The desired user ID is already taken.");

        var (status, response) = _exceptionMapper.Map(exception);

        status.Should().Be(StatusCodes.Status400BadRequest);
        response.Code.Should().Be(MatrixErrorCodes.UserInUse);
        response.Message.Should().Be(exception.Message);
    }

    [Fact]
    public void Map_RegistrationNotAllowedException_ReturnsForbiddenWithForbiddenCode()
    {
        var exception = new RegistrationNotAllowedException("Registration of 'guest' accounts is not supported.");

        var (status, response) = _exceptionMapper.Map(exception);

        status.Should().Be(StatusCodes.Status403Forbidden);
        response.Code.Should().Be(MatrixErrorCodes.Forbidden);
        response.Message.Should().Be(exception.Message);
    }

    [Fact]
    public void Map_UnknownException_ReturnsValidResponse()
    {
        var exception = new UnknownException();

        var (status, response) = _exceptionMapper.Map(exception);

        status.Should().Be(StatusCodes.Status500InternalServerError);
        response.Code.Should().Be(MatrixErrorCodes.Unknown);
        response.Message.Should().ContainEquivalentOf("unknown error");
    }

    private class UnknownException : Exception;
}