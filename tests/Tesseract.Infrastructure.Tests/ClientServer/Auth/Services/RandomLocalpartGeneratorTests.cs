using FluentAssertions;
using Tesseract.Domain.Users.Values;
using Tesseract.Infrastructure.ClientServer.Auth.Services;

namespace Tesseract.Infrastructure.Tests.ClientServer.Auth.Services;

public class RandomLocalpartGeneratorTests
{
    private readonly RandomLocalpartGenerator _generator = new();

    [Fact]
    public void Create_Always_ReturnsValidLocalpart()
    {
        var localpart = _generator.Create();

        Localpart.TryParse(localpart.Value, out _).Should().BeTrue();
    }

    [Fact]
    public void Create_CalledMultipleTimes_ReturnsUniqueLocalparts()
    {
        var first = _generator.Create();
        var second = _generator.Create();

        first.Should().NotBe(second);
    }
}