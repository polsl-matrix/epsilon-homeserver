using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Infrastructure.ClientServer.Discovery;

namespace Tesseract.Infrastructure.Tests.ClientServer.Discovery;

public class WellKnownRepositoryTests
{
    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationHasHomeserverBaseUrl_ReturnsDiscoveryInfo()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Matrix:Homeserver:BaseUrl", "https://hs.example.com" },
                { "Matrix:IdentityServer:BaseUrl", "https://is.example.com" },
            })
            .Build();

        var repository = new WellKnownRepository(configuration);

        var info = await repository.GetDiscoveryInfo(CancellationToken.None);

        info.Should().NotBeNull();
        info!.HomeserverBaseUrl.Should().Be("https://hs.example.com");
        info.IdentityServerBaseUrl.Should().Be("https://is.example.com");
    }

    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationHasOnlyHomeserverBaseUrl_ReturnsDiscoveryInfoWithNullIdentityServer()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Matrix:Homeserver:BaseUrl", "https://hs.example.com" },
            })
            .Build();

        var repository = new WellKnownRepository(configuration);

        var info = await repository.GetDiscoveryInfo(CancellationToken.None);

        info.Should().NotBeNull();
        info!.HomeserverBaseUrl.Should().Be("https://hs.example.com");
        info.IdentityServerBaseUrl.Should().BeNull();
    }

    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationMissingHomeserverBaseUrl_ReturnsNull()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var repository = new WellKnownRepository(configuration);

        var info = await repository.GetDiscoveryInfo(CancellationToken.None);

        info.Should().BeNull();
    }

    [Fact]
    public async Task GetDiscoveryInfo_ConfigurationHasEmptyHomeserverBaseUrl_ReturnsNull()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Matrix:Homeserver:BaseUrl", "" },
            })
            .Build();

        var repository = new WellKnownRepository(configuration);

        var info = await repository.GetDiscoveryInfo(CancellationToken.None);

        info.Should().BeNull();
    }
}