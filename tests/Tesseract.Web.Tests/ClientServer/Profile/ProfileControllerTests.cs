using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Web.ClientServer.Profile;

namespace Tesseract.Web.Tests.ClientServer.Profile;

public class ProfileControllerTests
{
    private readonly IMediator _mediator;
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _controller = new ProfileController(_mediator);
    }

    [Fact]
    public async Task GetDisplayName_RequestContainsUserId_PassesSameUserIdToMediator()
    {
        var response = new GetDisplayName.Response("Alice");
        GetDisplayName.Query? calledQuery = null;
        _mediator.Send(Arg.Any<GetDisplayName.Query>(), Arg.Any<CancellationToken>())
            .Returns(response)
            .AndDoes(call => calledQuery = call.Arg<GetDisplayName.Query>());

        await _controller.GetDisplayName("@alice:example.com", CancellationToken.None);

        calledQuery.Should().NotBeNull();
        calledQuery!.UserId.Should().Be("@alice:example.com");
    }

    [Fact]
    public async Task GetDisplayName_MediatorReturnsResponse_MapsValuesCorrectly()
    {
        _mediator.Send(Arg.Any<GetDisplayName.Query>(), Arg.Any<CancellationToken>())
            .Returns(new GetDisplayName.Response("Alice"));

        var result = await _controller.GetDisplayName("@alice:example.com", CancellationToken.None);

        result.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetDisplayName_CancellationTokenProvided_PassesSameTokenToMediator()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        _mediator.Send(Arg.Any<GetDisplayName.Query>(), cancellationToken)
            .Returns(new GetDisplayName.Response("Alice"));

        await _controller.GetDisplayName("@alice:example.com", cancellationToken);

        await _mediator.Received().Send(Arg.Any<GetDisplayName.Query>(), cancellationToken);
    }

    [Fact]
    public async Task GetDisplayName_MediatorThrowsException_PropagatesException()
    {
        var exception = new InvalidOperationException("Something went wrong.");
        _mediator.Send(Arg.Any<GetDisplayName.Query>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var act = async () => await _controller.GetDisplayName("@alice:example.com", CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(exception);
    }
}