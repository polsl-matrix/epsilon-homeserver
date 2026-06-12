using FluentAssertions;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Domain.Tests.Users;

public class HandleTests
{
    [Theory]
    [InlineData("jerry.smith", "localhost", "@jerry.smith:localhost")]
    [InlineData("marry/cole", "192.168.15.39:8391", "@marry/cole:192.168.15.39:8391")]
    public void ToString_Always_ReturnsValidTextHandle(string localpart, string domain, string expected)
    {
        var handle = new Handle(localpart, domain);

        var result = handle.ToString();

        result.Should().Be(expected);
    }

    [Fact]
    public void TryParse_WithEmptyString_ReturnsFalse()
    {
        var result = Handle.TryParse(string.Empty, out _);

        result.Should().BeFalse("empty string is not a valid handle");
    }

    [Fact]
    public void TryParse_WithNoAtSign_ReturnsFalse()
    {
        var result = Handle.TryParse("neo:zeros-n-ones", out _);

        result.Should().BeFalse("valid handles must contain @ sign");
    }

    [Fact]
    public void TryParse_WithNoSeparator_ReturnsFalse()
    {
        var result = Handle.TryParse("@neo_zeros-n-ones", out _);

        result.Should().BeFalse("valid handles must contain : sign");
    }

    [Theory]
    [InlineData("@jane+locust:server.io:1234", "jane+locust", "server.io:1234")]
    [InlineData("@jared.frog:192.168.44.12", "jared.frog", "192.168.44.12")]
    public void TryParse_WithValidHandle_ShouldSplitPartsCorrectlyAndReturnTrue(
        string input, string localpart, string domain)
    {
        var result = Handle.TryParse(input, out var handle);

        result.Should().BeTrue();
        handle.Should().NotBeNull();
        handle.Value.Localpart.Value.Should().Be(localpart);
        handle.Value.Domain.Value.Should().Be(domain);
    }

    [Theory]
    [InlineData("@sam^anda93:server.com:3802")]
    [InlineData("@blobfish:fe80:::3000")]
    public void TryParse_WithInvalidHandle_ReturnsFalse(string input)
    {
        var result = Handle.TryParse(input, out _);

        result.Should().BeFalse();
    }
}