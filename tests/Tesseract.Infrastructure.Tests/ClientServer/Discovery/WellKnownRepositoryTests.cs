using FluentAssertions;
using Microsoft.Extensions.Options;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Domain.Discovery.Values;
using Tesseract.Infrastructure.ClientServer.Discovery;

namespace Tesseract.Infrastructure.Tests.ClientServer.Discovery;

public class WellKnownRepositoryTests
{
    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationHasHomeserverBaseUrl_ReturnsDiscoveryInfo()
    {
        var options = Options.Create(new MatrixOptions
        {
            Homeserver = new HomeserverOptions { BaseUrl = "https://hs.example.com" },
            IdentityServer = new IdentityServerOptions { BaseUrl = "https://is.example.com" },
        });

        var repository = new WellKnownRepository(options);

        var info = await repository.GetDiscoveryInfo(CancellationToken.None);

        info.Should().NotBeNull();
        info!.HomeserverBaseUrl.Should().Be("https://hs.example.com");
        info.IdentityServerBaseUrl.Should().Be("https://is.example.com");
    }

    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationHasOnlyHomeserverBaseUrl_ReturnsDiscoveryInfoWithNullIdentityServer()
    {
        var options = Options.Create(new MatrixOptions
        {
            Homeserver = new HomeserverOptions { BaseUrl = "https://hs.example.com" },
        });

        var repository = new WellKnownRepository(options);

        var info = await repository.GetDiscoveryInfo(CancellationToken.None);

        info.Should().NotBeNull();
        info!.HomeserverBaseUrl.Should().Be("https://hs.example.com");
        info.IdentityServerBaseUrl.Should().BeNull();
    }

    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationMissingHomeserverBaseUrl_ReturnsNull()
    {
        var options = Options.Create(new MatrixOptions());

        var repository = new WellKnownRepository(options);

        var info = await repository.GetDiscoveryInfo(CancellationToken.None);

        info.Should().BeNull();
    }

    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationHasEmptyHomeserverBaseUrl_ThrowsInvalidOperationException()
    {
        var options = Options.Create(new MatrixOptions
        {
            Homeserver = new HomeserverOptions { BaseUrl = "" },
        });

        var repository = new WellKnownRepository(options);

        var act = () => repository.GetDiscoveryInfo(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
