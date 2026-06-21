using FluentAssertions;
using Tesseract.Domain.Common.Exceptions;
using Localpart = Tesseract.Domain.Common.Values.Localpart;

namespace Tesseract.Domain.Tests.Users;

public class LocalpartTests
{
    [Fact]
    public void Constructor_WithTrailingWhitespace_ShouldTrimValue()
    {
        const string value = "    john.doe    ";
        const string expected = "john.doe";

        var localpart = new Localpart(value);

        localpart.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("e")]
    [InlineData("meme.cat")]
    [InlineData("toby-123")]
    [InlineData("forward/back")]
    [InlineData("jake_cooks+social")]
    [InlineData("=kitty=")]
    public void Constructor_WithValidCharacters_ShouldInitializeCorrectly(string value)
    {
        var localpart = new Localpart(value);

        localpart.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("pineapple^pizza")]
    [InlineData("*ital:c")]
    [InlineData("UPPERcase")]
    [InlineData("\"mike\"")]
    [InlineData("&#53;")]
    [InlineData("jerry@terry?fied")]
    [InlineData("frühstückskäffchen")]
    public void Constructor_WithInvalidCharacters_ShouldThrowValidationException(string value)
    {
        var act = () => new Localpart(value);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Constructor_WithValueTooShort_ShouldThrowValidationException()
    {
        const string input = "";

        var act = () => new Localpart(input);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Constructor_WithValueExceedingMaxLength_ShouldThrowValidationException()
    {
        var input = new string('e', 256);

        var act = () => new Localpart(input);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("r0b0t5")]
    [InlineData("jerry.makes-stuff")]
    [InlineData(".skippy123")]
    public void ToString_Always_ReturnsActualValue(string value)
    {
        var localpart = new Localpart(value);

        var result = localpart.ToString();

        result.Should().Be(value);
    }

    [Theory]
    [InlineData("richart/d")]
    [InlineData("cooperative.wall393")]
    [InlineData("plenty-of-lasagna")]
    public void ImplicitOperator_WithValidValue_ShouldCreateLocalpart(string value)
    {
        Localpart localpart = value;

        localpart.Should().NotBeNull();
        localpart.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("smiley&doggo")]
    [InlineData("vibe~s")]
    public void ImplicitOperator_WithInvalidValue_ShouldThrowValidationException(string value)
    {
        Func<Localpart> act = () => value;

        act.Should().Throw<ValidationException>();
    }
}