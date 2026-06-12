using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tesseract.Application.ClientServer.Auth.Exceptions;
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
        // @formatter:on
    };

    [Fact]
    public void Map_UnknownException_ReturnsValidResponse()
    {
        var exception = new UnknownException();

        var (httpStatus, response) = _exceptionMapper.Map(exception);

        httpStatus.Should().Be(StatusCodes.Status500InternalServerError);
        response.Code.Should().Be(MatrixErrorCodes.Unknown);
        response.Message.Should().ContainEquivalentOf("unknown error");
    }

    [Theory]
    [MemberData(nameof(KnownExceptions))]
    public void Map_KnownException_ReturnsValidResponse(Exception exception, int status, string code, string? message)
    {
        var (httpStatus, response) = _exceptionMapper.Map(exception);

        httpStatus.Should().Be(status);
        response.Code.Should().Be(code);

        if (message is not null)
        {
            response.Message.Should().ContainEquivalentOf(message);
        }
    }

    private class UnknownException : Exception;
}