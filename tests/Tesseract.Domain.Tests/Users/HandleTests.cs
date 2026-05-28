using FluentAssertions;
using Tesseract.Domain.Common.Exceptions;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Domain.Tests.Users;

public class HandleTests
{
    [Theory]
    [InlineData("jerry.smith", "localhost", "@jerry.smith:localhost")]
    [InlineData("marry/cole", "192.168.15.39:8391", "@marry/cole:192.168.15.39:8391")]
    public void ToString_Always_ShouldReturnValidTextHandle(string localpart, string domain, string expected)
    {
        var handle = new Handle(localpart, domain);

        var result = handle.ToString();

        result.Should().Be(expected);
    }

    [Fact]
    public void Parse_WithEmptyString_ShouldThrowValidationException()
    {
        var act = () => Handle.Parse(string.Empty);

        var thrown = act.Should().Throw<ValidationException>();
        thrown.Which.Message.Should().ContainEquivalentOf("empty");
    }

    [Fact]
    public void Parse_WithNoAtSign_ShouldThrowValidationException()
    {
        var act = () => Handle.Parse("neo:zeros-n-ones");

        var thrown = act.Should().Throw<ValidationException>();
        thrown.Which.Message.Should().ContainEquivalentOf("invalid handle format");
    }

    [Fact]
    public void Parse_WithNoSeparator_ShouldThrowValidationException()
    {
        var act = () => Handle.Parse("@neo_zeros-n-ones");

        var thrown = act.Should().Throw<ValidationException>();
        thrown.Which.Message.Should().ContainEquivalentOf("invalid handle format");
    }

    [Theory]
    [InlineData("@jane+locust:server.io:1234", "jane+locust", "server.io:1234")]
    [InlineData("@jared.frog:192.168.44.12", "jared.frog", "192.168.44.12")]
    public void Parse_WithValidHandle_ShouldSplitPartsCorrectly(string input, string localpart, string domain)
    {
        var handle = Handle.Parse(input);

        handle.Localpart.Value.Should().Be(localpart);
        handle.Domain.Value.Should().Be(domain);
    }

    [Theory]
    [InlineData("@sam^anda93:server.com:3802", "localpart")]
    [InlineData("@blobfish:fe80:::3000", "domain")]
    public void Parse_WithInvalidHandle_ShouldThrowValidationException(string input, string part)
    {
        var act = () => Handle.Parse(input);

        var thrown = act.Should().Throw<ValidationException>();
        thrown.Which.Message.Should().ContainEquivalentOf(part);
    }
}