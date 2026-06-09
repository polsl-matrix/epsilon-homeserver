using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Text.Json;
using Tesseract.Web.Common.Errors;
using Tesseract.Web.Common.Errors.Contracts;

namespace Tesseract.Web.Tests.Common.Errors;

public class GlobalExceptionHandlerTests
{
    private readonly IMatrixExceptionMapper _exceptionMapper;
    private readonly GlobalExceptionHandler _exceptionHandler;

    public GlobalExceptionHandlerTests()
    {
        _exceptionMapper = Substitute.For<IMatrixExceptionMapper>();
        _exceptionHandler = new GlobalExceptionHandler(_exceptionMapper);
    }

    public static TheoryData<Exception, int, MatrixErrorResponse> ValidMappings => new()
    {
        // @formatter:off
        { new Exception(), StatusCodes.Status500InternalServerError, new MatrixErrorResponse(MatrixErrorCodes.Unknown, "An unknown error has occurred.") },
        // @formatter:on
    };

    [Theory]
    [MemberData(nameof(ValidMappings))]
    public async Task TryHandleAsync_Always_SetsStatusCode_WritesValidResponse_ReturnsTrue(
        Exception exception, int status, MatrixErrorResponse response)
    {
        // Arrange
        _exceptionMapper.Map(Arg.Any<Exception>())
            .Returns((status, response));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        var result = await _exceptionHandler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.StatusCode.Should().Be(status);
        context.Response.ContentType.Should().ContainEquivalentOf("application/json");
        result.Should().BeTrue("middleware handled the exception");

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var actualResponse = JsonSerializer.Deserialize<MatrixErrorResponse>(body);

        actualResponse.Should().Be(response);
    }
}