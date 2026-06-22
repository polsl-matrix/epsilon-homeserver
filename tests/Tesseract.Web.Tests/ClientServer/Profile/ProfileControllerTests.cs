using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;
using Tesseract.Web.ClientServer.Profile;
using Tesseract.Web.ClientServer.Profile.Contracts;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.Tests.ClientServer.Profile;

public sealed class ProfileControllerTests
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _controller = new ProfileController(_mediator, _currentUser);
    }

    [Fact]
    public async Task UpdateDisplayName_ValidRequest_SendsCommandWithCurrentUserAndTargetUser()
    {
        var currentUserId = UserId.Random();
        var request = new UpdateDisplayNameRequest
        {
            DisplayName = "Alice",
        };
        _currentUser.Id.Returns(currentUserId);

        await _controller.UpdateDisplayName(request, "@alice:example.com", CancellationToken.None);

        await _mediator.Received().Send(
            Arg.Is<UpdateDisplayName.Command>(command =>
                command.UserId == currentUserId &&
                command.UserHandle == "@alice:example.com" &&
                command.DisplayName == "Alice"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDisplayName_MediatorCompletes_ReturnsEmptyObject()
    {
        _currentUser.Id.Returns(UserId.Random());
        var request = new UpdateDisplayNameRequest
        {
            DisplayName = "Alice",
        };

        var result = await _controller.UpdateDisplayName(request, "@alice:example.com", CancellationToken.None);

        result.GetType().GetProperties().Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateDisplayName_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        _currentUser.Id.Returns(UserId.Random());
        var request = new UpdateDisplayNameRequest
        {
            DisplayName = "Alice",
        };

        await _controller.UpdateDisplayName(request, "@alice:example.com", cancellationSource.Token);

        await _mediator.Received().Send(Arg.Any<UpdateDisplayName.Command>(), cancellationSource.Token);
    }

    [Fact]
    public async Task UpdateDisplayName_MediatorThrowsProfileUpdateForbiddenException_PropagatesException()
    {
        var exception = new CannotUpdateOtherUserProfileException("@alice:example.com", "@bob:example.com");
        _currentUser.Id.Returns(UserId.Random());
        _mediator.Send(Arg.Any<UpdateDisplayName.Command>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw exception);

        var act = () => _controller.UpdateDisplayName(
            new UpdateDisplayNameRequest
            {
                DisplayName = "Bob",
            },
            "@bob:example.com",
            CancellationToken.None);

        await act.Should().ThrowAsync<CannotUpdateOtherUserProfileException>();
    }

    [Fact]
    public async Task UpdateAvatarUrl_ValidRequest_SendsCommandWithCurrentUserAndTargetUser()
    {
        var currentUserId = UserId.Random();
        var request = new UpdateAvatarUrlRequest
        {
            AvatarUrl = "mxc://example.com/avatar",
        };
        _currentUser.Id.Returns(currentUserId);

        await _controller.UpdateAvatarUrl(request, "@alice:example.com", CancellationToken.None);

        await _mediator.Received().Send(
            Arg.Is<UpdateAvatarUrl.Command>(command =>
                command.UserId == currentUserId &&
                command.UserHandle == "@alice:example.com" &&
                command.AvatarUrl == "mxc://example.com/avatar"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAvatarUrl_MediatorCompletes_ReturnsEmptyObject()
    {
        _currentUser.Id.Returns(UserId.Random());
        var request = new UpdateAvatarUrlRequest
        {
            AvatarUrl = "mxc://example.com/avatar",
        };

        var result = await _controller.UpdateAvatarUrl(request, "@alice:example.com", CancellationToken.None);

        result.GetType().GetProperties().Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAvatarUrl_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        _currentUser.Id.Returns(UserId.Random());
        var request = new UpdateAvatarUrlRequest
        {
            AvatarUrl = "mxc://example.com/avatar",
        };

        await _controller.UpdateAvatarUrl(request, "@alice:example.com", cancellationSource.Token);

        await _mediator.Received().Send(Arg.Any<UpdateAvatarUrl.Command>(), cancellationSource.Token);
    }

    [Fact]
    public async Task UpdateAvatarUrl_MediatorThrowsProfileUpdateForbiddenException_PropagatesException()
    {
        var exception = new CannotUpdateOtherUserProfileException("@alice:example.com", "@bob:example.com");
        _currentUser.Id.Returns(UserId.Random());
        _mediator.Send(Arg.Any<UpdateAvatarUrl.Command>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw exception);

        var act = () => _controller.UpdateAvatarUrl(
            new UpdateAvatarUrlRequest
            {
                AvatarUrl = "mxc://example.com/bob",
            },
            "@bob:example.com",
            CancellationToken.None);

        await act.Should().ThrowAsync<CannotUpdateOtherUserProfileException>();
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
        calledQuery.UserHandle.Should().Be("@alice:example.com");
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