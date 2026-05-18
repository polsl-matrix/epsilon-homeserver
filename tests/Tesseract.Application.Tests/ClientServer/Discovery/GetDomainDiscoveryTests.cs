using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Discovery.Abstractions;

namespace Tesseract.Application.Tests.ClientServer.Discovery;

public class GetDomainDiscoveryTests
{
    private readonly IWellKnownRepository _repository;
    private readonly GetDomainDiscovery.Handler _handler;

    public GetDomainDiscoveryTests()
    {
        _repository = Substitute.For<IWellKnownRepository>();
        _handler = new GetDomainDiscovery.Handler(_repository);
    }

    [Fact]
    public async Task Handle_RepositoryReturnsInfo_ReturnsResponseWithSameData()
    {
        _repository.GetDiscoveryInfo(Arg.Any<CancellationToken>())
            .Returns(new DiscoveryInfo("https://hs.example.com", "https://is.example.com"));

        var response = await _handler.Handle(new GetDomainDiscovery.Query(), CancellationToken.None);

        response.Should().NotBeNull();
        response!.HomeserverBaseUrl.Should().Be("https://hs.example.com");
        response.IdentityServerBaseUrl.Should().Be("https://is.example.com");
    }

    [Fact]
    public async Task Handle_RepositoryReturnsNull_ReturnsNull()
    {
        _repository.GetDiscoveryInfo(Arg.Any<CancellationToken>())
            .Returns((DiscoveryInfo?)null);

        var response = await _handler.Handle(new GetDomainDiscovery.Query(), CancellationToken.None);

        response.Should().BeNull();
    }

    [Fact]
    public async Task Handle_RepositoryReturnsInfoWithoutIdentityServer_ResponseHasNullIdentityServer()
    {
        _repository.GetDiscoveryInfo(Arg.Any<CancellationToken>())
            .Returns(new DiscoveryInfo("https://hs.example.com", null));

        var response = await _handler.Handle(new GetDomainDiscovery.Query(), CancellationToken.None);

        response.Should().NotBeNull();
        response!.IdentityServerBaseUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenToRepository()
    {
        var cancellationSource = new CancellationTokenSource();

        await _handler.Handle(new GetDomainDiscovery.Query(), cancellationSource.Token);

        await _repository.Received().GetDiscoveryInfo(cancellationSource.Token);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException()
    {
        var exception = new InvalidOperationException("Something went wrong.");

        _repository.GetDiscoveryInfo(Arg.Any<CancellationToken>())
            .Throws(exception);

        var act = () => _handler.Handle(new GetDomainDiscovery.Query(), CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(exception);
    }
}