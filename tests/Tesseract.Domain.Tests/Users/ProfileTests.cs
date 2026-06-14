using FluentAssertions;
using Tesseract.Domain.Users;

namespace Tesseract.Domain.Tests.Users;

public class ProfileTests
{
    [Fact]
    public void Constructor_ValuesProvided_StoresValues()
    {
        var userId = UserId.Random();

        var profile = new Profile(userId, "Alice", "mxc://example.com/avatar");

        profile.UserId.Should().Be(userId);
        profile.DisplayName.Should().Be("Alice");
        profile.AvatarUrl.Should().Be("mxc://example.com/avatar");
    }

    [Fact]
    public void Constructor_ProfileFieldsAreNull_StoresNullProfileFields()
    {
        var userId = UserId.Random();

        var profile = new Profile(userId, null, null);

        profile.UserId.Should().Be(userId);
        profile.DisplayName.Should().BeNull();
        profile.AvatarUrl.Should().BeNull();
    }
}