using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Discovery.Abstractions;
using Tesseract.Application.ClientServer.Discovery.Models;

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
            .Returns(new DiscoveryInfo(new Uri("https://hs.example.com/"), new Uri("https://is.example.com/")));

        var response = await _handler.Handle(new GetDomainDiscovery.Query(), CancellationToken.None);

        response.HomeserverBaseUrl.Should().Be("https://hs.example.com/");
        response.IdentityServerBaseUrl.Should().Be("https://is.example.com/");
    }

    [Fact]
    public async Task Handle_RepositoryReturnsNull_ThrowsInvalidOperationException()
    {
        _repository.GetDiscoveryInfo(Arg.Any<CancellationToken>())
            .Returns((DiscoveryInfo?)null);

        var act = () => _handler.Handle(new GetDomainDiscovery.Query(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_RepositoryReturnsInfoWithoutIdentityServer_ResponseHasNullIdentityServer()
    {
        _repository.GetDiscoveryInfo(Arg.Any<CancellationToken>())
            .Returns(new DiscoveryInfo(new Uri("https://hs.example.com/"), null));

        var response = await _handler.Handle(new GetDomainDiscovery.Query(), CancellationToken.None);

        response.IdentityServerBaseUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenToRepository()
    {
        var cancellationSource = new CancellationTokenSource();

        _repository.GetDiscoveryInfo(Arg.Any<CancellationToken>())
            .Returns(new DiscoveryInfo(new Uri("https://hs.example.com/"), null));

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