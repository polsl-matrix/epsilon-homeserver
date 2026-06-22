using FluentAssertions;
using MediatR;
using NSubstitute;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Web.ClientServer.Profile;

namespace Tesseract.Web.Tests.ClientServer.Profile;

public sealed class ProfileControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        _controller = new ProfileController(_sender);
    }

    [Fact]
    public async Task GetAvatarUrl_ValidRequest_SendsQueryWithUserId()
    {
        const string userId = "@alice:example.com";
        _sender.Send(Arg.Any<GetAvatarUrl.Query>(), Arg.Any<CancellationToken>())
            .Returns(new GetAvatarUrl.Response("mxc://example.com/avatar"));

        await _controller.GetAvatarUrl(userId, CancellationToken.None);

        await _sender.Received().Send(
            Arg.Is<GetAvatarUrl.Query>(query => query.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAvatarUrl_QueryReturnsAvatarUrl_ReturnsAvatarUrlResponse()
    {
        const string avatarUrl = "mxc://example.com/avatar";
        _sender.Send(Arg.Any<GetAvatarUrl.Query>(), Arg.Any<CancellationToken>())
            .Returns(new GetAvatarUrl.Response(avatarUrl));

        var result = await _controller.GetAvatarUrl("@alice:example.com", CancellationToken.None);

        result.AvatarUrl.Should().Be(avatarUrl);
    }

    [Fact]
    public async Task GetAvatarUrl_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        _sender.Send(Arg.Any<GetAvatarUrl.Query>(), Arg.Any<CancellationToken>())
            .Returns(new GetAvatarUrl.Response("mxc://example.com/avatar"));

        await _controller.GetAvatarUrl("@alice:example.com", cancellationSource.Token);

        await _sender.Received().Send(Arg.Any<GetAvatarUrl.Query>(), cancellationSource.Token);
    }

    [Fact]
    public async Task GetAvatarUrl_SenderThrowsProfileFieldNotFoundException_PropagatesException()
    {
        var exception = new ProfileFieldNotFoundException("@alice:example.com", "avatar_url");
        _sender.Send(Arg.Any<GetAvatarUrl.Query>(), Arg.Any<CancellationToken>())
            .Returns<Task<GetAvatarUrl.Response>>(_ => throw exception);

        var act = () => _controller.GetAvatarUrl("@alice:example.com", CancellationToken.None);

        await act.Should().ThrowAsync<ProfileFieldNotFoundException>();
    }
}