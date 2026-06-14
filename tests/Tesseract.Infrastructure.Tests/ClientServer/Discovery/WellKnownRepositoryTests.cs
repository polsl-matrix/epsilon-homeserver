using FluentAssertions;
using Microsoft.Extensions.Options;
using Tesseract.Infrastructure.ClientServer.Discovery;
using Tesseract.Infrastructure.ClientServer.Discovery.Configuration;

namespace Tesseract.Infrastructure.Tests.ClientServer.Discovery;

public class WellKnownRepositoryTests
{
    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationHasHomeserverBaseUrl_ReturnsDiscoveryInfo()
    {
        var options = Options.Create(new DiscoveryOptions
        {
            HomeserverBaseUrl = new Uri("https://hs.example.com/"),
            IdentityServerBaseUrl = new Uri("https://is.example.com/"),
        });

        var repository = new WellKnownRepository(options);

        var info = await repository.GetDiscoveryInfo(CancellationToken.None);

        info.Should().NotBeNull();
        info.HomeserverBaseUrl.Should().Be("https://hs.example.com/");
        info.IdentityServerBaseUrl.Should().Be("https://is.example.com/");
    }

    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationHasOnlyHomeserverBaseUrl_ReturnsDiscoveryInfoWithNullIdentityServer()
    {
        var options = Options.Create(new DiscoveryOptions
        {
            HomeserverBaseUrl = new Uri("https://hs.example.com/"),
        });

        var repository = new WellKnownRepository(options);

        var info = await repository.GetDiscoveryInfo(CancellationToken.None);

        info.Should().NotBeNull();
        info.HomeserverBaseUrl.Should().Be("https://hs.example.com/");
        info.IdentityServerBaseUrl.Should().BeNull();
    }
}