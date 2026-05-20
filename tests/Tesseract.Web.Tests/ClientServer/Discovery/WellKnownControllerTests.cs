using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Web.ClientServer.Discovery;
using Tesseract.Web.ClientServer.Discovery.Contracts;

namespace Tesseract.Web.Tests.ClientServer.Discovery;

public class WellKnownControllerTests
{
    private readonly IMediator _mediator;
    private readonly WellKnownController _controller;

    public WellKnownControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _controller = new WellKnownController(_mediator);
    }

    [Fact]
    public async Task GetDomainDiscovery_MediatorReturnsResult_ReturnsResponseWithSameData()
    {
        _mediator.Send(Arg.Any<GetDomainDiscovery.Query>(), Arg.Any<CancellationToken>())
            .Returns(new GetDomainDiscovery.Response("https://hs.example.com", "https://is.example.com"));

        var response = await _controller.GetDomainDiscovery(CancellationToken.None);

        response.Value.Should().NotBeNull();
        response.Result.Should().BeNull();
        response.Value!.Homeserver.BaseUrl.Should().Be("https://hs.example.com");
        response.Value.IdentityServer!.BaseUrl.Should().Be("https://is.example.com");
    }

    [Fact]
    public async Task GetDomainDiscovery_MediatorReturnsNull_ReturnsNotFound()
    {
        _mediator.Send(Arg.Any<GetDomainDiscovery.Query>(), Arg.Any<CancellationToken>())
            .Returns((GetDomainDiscovery.Response?)null);

        var response = await _controller.GetDomainDiscovery(CancellationToken.None);

        response.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetDomainDiscovery_MediatorReturnsResultWithoutIdentityServer_ResponseDoesNotContainIdentityServer()
    {
        _mediator.Send(Arg.Any<GetDomainDiscovery.Query>(), Arg.Any<CancellationToken>())
            .Returns(new GetDomainDiscovery.Response("https://hs.example.com", null));

        var response = await _controller.GetDomainDiscovery(CancellationToken.None);

        response.Value.Should().NotBeNull();
        response.Result.Should().BeNull();
        response.Value!.IdentityServer.Should().BeNull();
    }

    [Fact]
    public async Task GetDomainDiscovery_CancellationTokenProvided_PassesSameTokenToMediator()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        _mediator.Send(Arg.Any<GetDomainDiscovery.Query>(), cancellationToken)
            .Returns(new GetDomainDiscovery.Response("https://hs.example.com", null));

        await _controller.GetDomainDiscovery(cancellationToken);

        await _mediator.Received().Send(Arg.Any<GetDomainDiscovery.Query>(), cancellationToken);
    }

    [Fact]
    public async Task GetDomainDiscovery_MediatorThrowsException_PropagatesException()
    {
        var exception = new InvalidOperationException("Something went wrong.");

        _mediator.Send(Arg.Any<GetDomainDiscovery.Query>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var act = () => _controller.GetDomainDiscovery(CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(exception);
    }
}