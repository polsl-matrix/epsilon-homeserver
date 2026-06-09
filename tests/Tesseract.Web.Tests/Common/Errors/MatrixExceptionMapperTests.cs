using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tesseract.Web.Common.Errors.Contracts;
using Tesseract.Web.Common.Errors.Interfaces;

namespace Tesseract.Web.Tests.Common.Errors;

public class MatrixExceptionMapperTests
{
    private readonly MatrixExceptionMapper _exceptionMapper = new();

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