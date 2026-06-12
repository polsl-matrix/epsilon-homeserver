using FluentAssertions;
using Tesseract.Infrastructure.ClientServer.Auth;

namespace Tesseract.Infrastructure.Tests.ClientServer.Auth;

public class Sha256HashServiceTests
{
    private readonly Sha256HashService _service = new();

    [Fact]
    public async Task HashAsync_ReturnsValidSha256()
    {
        const string input = "tes7!";

        var hash = await _service.HashAsync(input, CancellationToken.None);

        hash.Should().HaveCount(32, "result hash of SHA256 contains 32 bytes of data");
    }
}