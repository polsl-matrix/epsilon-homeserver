using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Web.ClientServer.Discovery;

namespace Tesseract.Web.Tests.ClientServer.Discovery;

public class VersionControllerTests
{
    private readonly IMediator _mediator;
    private readonly VersionController _controller;

    public VersionControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _controller = new VersionController(_mediator);
    }

    [Theory]
    [InlineData((object)new string[] { })]
    [InlineData((object)new[] { "r0.0.1", "v1.1", "v.1.18" })]
    public async Task GetSupportedVersions_MediatorReturnsVersions_ReturnsResponseWithSameVersions(string[] versions)
    {
        _mediator.Send(Arg.Any<GetSupportedVersions.Query>(), Arg.Any<CancellationToken>())
            .Returns(new GetSupportedVersions.Response(versions));

        var response = await _controller.GetSupportedVersions(CancellationToken.None);

        response.Versions.Should().Equal(versions);
    }

    [Fact]
    public async Task GetSupportedVersions_CancellationTokenProvided_PassesSameTokenToMediator()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        _mediator.Send(Arg.Any<GetSupportedVersions.Query>(), cancellationToken)
            .Returns(new GetSupportedVersions.Response(["r0.0.1", "v1.1", "v.1.18-alpha"]));

        await _controller.GetSupportedVersions(cancellationToken);

        await _mediator.Received().Send(Arg.Any<GetSupportedVersions.Query>(), cancellationToken);
    }

    [Fact]
    public async Task GetSupportedVersions_MediatorThrowsException_PropagatesException()
    {
        var exception = new InvalidOperationException("Something went wrong.");

        _mediator.Send(Arg.Any<GetSupportedVersions.Query>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var act = () => _controller.GetSupportedVersions(CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(exception);
    }
}