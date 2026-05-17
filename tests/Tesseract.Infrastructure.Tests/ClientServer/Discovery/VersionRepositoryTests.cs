using FluentAssertions;
using System.Text.RegularExpressions;
using Tesseract.Infrastructure.ClientServer.Discovery;

namespace Tesseract.Infrastructure.Tests.ClientServer.Discovery;

public partial class VersionRepositoryTests
{
    private readonly VersionRepository _repository = new();

    [GeneratedRegex(@"^v\d+\.\d+(?:-[a-z]+)?$", RegexOptions.Singleline)]
    private static partial Regex MatrixVersionRegex();

    [Fact]
    public async Task GetSupportedVersions_RepositoryCalled_ReturnsAtLeastOneVersion()
    {
        var versions = await _repository.GetSupportedVersions(CancellationToken.None);

        versions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSupportedVersions_RepositoryCalled_ReturnsVersionsMatchingExpectedFormat()
    {
        var versions = await _repository.GetSupportedVersions(CancellationToken.None);

        versions.Should().AllSatisfy(version =>
        {
            version.Should().MatchRegex(MatrixVersionRegex());
        });
    }
}