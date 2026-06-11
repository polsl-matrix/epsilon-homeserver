using FluentAssertions;
using Tesseract.Infrastructure.ClientServer.Auth;

namespace Tesseract.Infrastructure.Tests.ClientServer.Auth;

public class OpaqueTokenServiceTests
{
    private readonly OpaqueTokenService _service = new();

    [Fact]
    public void OpaqueTokenService_GeneratesSecureEnoughSequences() =>
        OpaqueTokenService.RawByteCount.Should().BeGreaterThanOrEqualTo(32);

    [Fact]
    public async Task Create_Always_ReturnsValidBase64StringWithExpectedLength()
    {
        // Arrange
        var length = GetBase64EncodedLength(OpaqueTokenService.RawByteCount);

        // Act
        var token = await _service.Create(CancellationToken.None);

        // Assert
        token.Length.Should().Be(length, "{0}-byte Base64 encoded sequence should contain {1} bytes",
            OpaqueTokenService.RawByteCount, length);
    }

    private static int GetBase64EncodedLength(int length)
    {
        // Base64 uses only 6 bits per char, which increases
        // the length of the message by a factor of 8/6.
        const double factor = 8 / 6.0;
        return (int)Math.Ceiling(length * factor);
    }

    [Fact]
    public async Task Create_CalledMultipleTimes_ReturnsUniqueTokens()
    {
        var a = await _service.Create(CancellationToken.None);
        var b = await _service.Create(CancellationToken.None);

        a.Should().NotBe(b, "tokens must be unique");
    }
}