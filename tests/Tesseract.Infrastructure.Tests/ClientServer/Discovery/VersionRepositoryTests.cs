using FluentAssertions;
using System.Text.RegularExpressions;
using Tesseract.Infrastructure.ClientServer.Discovery;

namespace Tesseract.Infrastructure.Tests.ClientServer.Discovery;

public partial class VersionRepositoryTests
{
    private readonly VersionRepository _repository = new();

    [GeneratedRegex(@"^v\d+\.\d+(?:-[a-z])?$", RegexOptions.Singleline)]
    private static partial Regex MatrixVersionRegex();

    [Fact]
    public async Task GetSupportedVersion_Always_ReturnsAtLeastOneVersion()
    {
        // Act
        var versions = await _repository
            .GetSupportedVersions(CancellationToken.None);

        // Assert
        versions.Should().NotBeEmpty(
            "the server must provide at least one version to allow clients to complete the version negotiation handshake");
    }

    [Fact]
    public async Task GetSupportedVersions_Always_FollowValidScheme()
    {
        // Act
        var versions = await _repository
            .GetSupportedVersions(CancellationToken.None);

        // Assert
        versions.Should().AllSatisfy(version =>
        {
            version.Should().MatchRegex(MatrixVersionRegex(),
                "versions must follow Matrix's semantic versioning schema");
        });
    }
}