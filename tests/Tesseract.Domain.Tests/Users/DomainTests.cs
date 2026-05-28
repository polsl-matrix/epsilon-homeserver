using FluentAssertions;
using Tesseract.Domain.Common.Exceptions;
using VDomain = Tesseract.Domain.Users.Values.Domain;

namespace Tesseract.Domain.Tests.Users;

public class DomainTests
{
    [Fact]
    public void Constructor_WithTrailingWhitespace_ShouldTrimValue()
    {
        const string value = "    test.thanks    ";
        const string expected = "test.thanks";

        var domain = new VDomain(value);

        domain.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("server.local:3901")]
    [InlineData("192.168.15.15")]
    [InlineData("[fe80::df89:d0c0]")]
    [InlineData("grasshopper:3526")]
    [InlineData("192.168.5.5:5754")]
    [InlineData("[fe80::29fc:39f]:27930")]
    public void Constructor_WithValidSequences_ShouldInitializeCorrectly(string value)
    {
        var domain = new VDomain(value);

        domain.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("local/host")]
    [InlineData("server?local:3901")]
    [InlineData("192.168.15.15:server")]
    [InlineData("fe80::df89:d0c0")]
    [InlineData("grasshopper:feed")]
    [InlineData("fe80::29fc:39f:27930")]
    [InlineData("[::]:390891")]
    [InlineData("[fe80:111:1111:1111:1111:1111:1111:1111:1111:1111]:390891")]
    public void Constructor_WithInvalidSequences_ShouldThrowValidationException(string value)
    {
        var act = () => new VDomain(value);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("epsilon.local:8374")]
    [InlineData("127.0.0.1")]
    [InlineData("[fe80::dd:3]:1892")]
    public void ToString_Always_ShouldReturnActualValue(string value)
    {
        var domain = new VDomain(value);

        var result = domain.ToString();

        result.Should().Be(value);
    }

    [Theory]
    [InlineData("172.16.81.30")]
    [InlineData("[fe80::e232:d902]")]
    [InlineData("rabbithole:3526")]
    public void ImplicitOperator_WithValidValue_ShouldCreateLocalpart(string value)
    {
        VDomain domain = value;

        domain.Should().NotBeNull();
        domain.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("fe80::df89:d0c0")]
    [InlineData("cabbage:carrot")]
    public void ImplicitOperator_WithInvalidValue_ShouldThrowValidationException(string value)
    {
        Func<VDomain> act = () => value;

        act.Should().Throw<ValidationException>();
    }
}