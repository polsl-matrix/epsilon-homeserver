using FluentAssertions;
using MediatR;
using NSubstitute;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;
using Tesseract.Web.ClientServer.Profile;
using Tesseract.Web.ClientServer.Profile.Contracts;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.Tests.ClientServer.Profile;

public sealed class ProfileControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        _controller = new ProfileController(_sender, _currentUser);
    }

    [Fact]
    public async Task UpdateAvatarUrl_ValidRequest_SendsCommandWithCurrentUserAndTargetUser()
    {
        var currentUserId = UserId.Random();
        var request = new UpdateAvatarUrlRequest { AvatarUrl = "mxc://example.com/avatar" };
        _currentUser.Id.Returns(currentUserId);

        await _controller.UpdateAvatarUrl("@alice:example.com", request, CancellationToken.None);

        await _sender.Received().Send(
            Arg.Is<UpdateAvatarUrl.Command>(command =>
                command.AuthenticatedUserId == currentUserId &&
                command.UserId == "@alice:example.com" &&
                command.AvatarUrl == "mxc://example.com/avatar"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAvatarUrl_NullAvatarUrl_SendsNullAvatarUrl()
    {
        var currentUserId = UserId.Random();
        var request = new UpdateAvatarUrlRequest { AvatarUrl = null };
        _currentUser.Id.Returns(currentUserId);

        await _controller.UpdateAvatarUrl("@alice:example.com", request, CancellationToken.None);

        await _sender.Received().Send(
            Arg.Is<UpdateAvatarUrl.Command>(command => command.AvatarUrl == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAvatarUrl_SenderCompletes_ReturnsEmptyObject()
    {
        _currentUser.Id.Returns(UserId.Random());
        var request = new UpdateAvatarUrlRequest { AvatarUrl = "mxc://example.com/avatar" };

        var result = await _controller.UpdateAvatarUrl("@alice:example.com", request, CancellationToken.None);

        result.GetType().GetProperties().Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAvatarUrl_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        _currentUser.Id.Returns(UserId.Random());
        var request = new UpdateAvatarUrlRequest { AvatarUrl = "mxc://example.com/avatar" };

        await _controller.UpdateAvatarUrl("@alice:example.com", request, cancellationSource.Token);

        await _sender.Received().Send(Arg.Any<UpdateAvatarUrl.Command>(), cancellationSource.Token);
    }

    [Fact]
    public async Task UpdateAvatarUrl_SenderThrowsProfileUpdateForbiddenException_PropagatesException()
    {
        var exception = new ProfileUpdateForbiddenException("@alice:example.com", "@bob:example.com");
        _currentUser.Id.Returns(UserId.Random());
        _sender.Send(Arg.Any<UpdateAvatarUrl.Command>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw exception);

        var act = () => _controller.UpdateAvatarUrl(
            "@bob:example.com",
            new UpdateAvatarUrlRequest { AvatarUrl = "mxc://example.com/bob" },
            CancellationToken.None);

        await act.Should().ThrowAsync<ProfileUpdateForbiddenException>();
    }
}