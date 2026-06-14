using FluentAssertions;
using Tesseract.Infrastructure.ClientServer.Auth.Services;

namespace Tesseract.Infrastructure.Tests.ClientServer.Auth.Services;

public class RandomDeviceIdGeneratorTests
{
    private readonly RandomDeviceIdGenerator _generator = new();

    [Fact]
    public void Create_Always_ReturnsNonEmptyDeviceId()
    {
        var deviceId = _generator.Create();

        deviceId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_CalledMultipleTimes_ReturnsUniqueDeviceIds()
    {
        var first = _generator.Create();
        var second = _generator.Create();

        first.Should().NotBe(second);
    }
}