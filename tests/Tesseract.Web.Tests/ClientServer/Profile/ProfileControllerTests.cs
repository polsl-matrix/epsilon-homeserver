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
    public async Task UpdateDisplayName_ValidRequest_SendsCommandWithCurrentUserAndTargetUser()
    {
        var currentUserId = UserId.Random();
        var request = new UpdateDisplayNameRequest { DisplayName = "Alice" };
        _currentUser.Id.Returns(currentUserId);

        await _controller.UpdateDisplayName("@alice:example.com", request, CancellationToken.None);

        await _sender.Received().Send(
            Arg.Is<UpdateDisplayName.Command>(command =>
                command.AuthenticatedUserId == currentUserId &&
                command.UserId == "@alice:example.com" &&
                command.DisplayName == "Alice"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDisplayName_NullDisplayName_SendsNullDisplayName()
    {
        var currentUserId = UserId.Random();
        var request = new UpdateDisplayNameRequest { DisplayName = null };
        _currentUser.Id.Returns(currentUserId);

        await _controller.UpdateDisplayName("@alice:example.com", request, CancellationToken.None);

        await _sender.Received().Send(
            Arg.Is<UpdateDisplayName.Command>(command => command.DisplayName == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDisplayName_SenderCompletes_ReturnsEmptyObject()
    {
        _currentUser.Id.Returns(UserId.Random());
        var request = new UpdateDisplayNameRequest { DisplayName = "Alice" };

        var result = await _controller.UpdateDisplayName("@alice:example.com", request, CancellationToken.None);

        result.GetType().GetProperties().Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateDisplayName_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        _currentUser.Id.Returns(UserId.Random());
        var request = new UpdateDisplayNameRequest { DisplayName = "Alice" };

        await _controller.UpdateDisplayName("@alice:example.com", request, cancellationSource.Token);

        await _sender.Received().Send(Arg.Any<UpdateDisplayName.Command>(), cancellationSource.Token);
    }

    [Fact]
    public async Task UpdateDisplayName_SenderThrowsProfileUpdateForbiddenException_PropagatesException()
    {
        var exception = new ProfileUpdateForbiddenException("@alice:example.com", "@bob:example.com");
        _currentUser.Id.Returns(UserId.Random());
        _sender.Send(Arg.Any<UpdateDisplayName.Command>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw exception);

        var act = () => _controller.UpdateDisplayName(
            "@bob:example.com",
            new UpdateDisplayNameRequest { DisplayName = "Bob" },
            CancellationToken.None);

        await act.Should().ThrowAsync<ProfileUpdateForbiddenException>();
    }
}