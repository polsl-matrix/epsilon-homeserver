using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Profile;

public sealed class UpdateDisplayNameTests
{
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly UpdateDisplayName.Handler _handler;

    public UpdateDisplayNameTests()
    {
        _handler = new UpdateDisplayName.Handler(_profileRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_OwnerUpdate_UpsertsDisplayName()
    {
        var user = CreateUser("alice", "example.com");
        var command = new UpdateDisplayName.Command(user.Id, user.Handle.ToString(), "Alice");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        await _profileRepository.Received().UpsertDisplayNameAsync(
            Arg.Is<UserId>(id => id == user.Id),
            "Alice",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DifferentTargetUser_ThrowsProfileUpdateForbiddenException()
    {
        var user = CreateUser("alice", "example.com");
        var command = new UpdateDisplayName.Command(user.Id, "@bob:example.com", "Bob");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var act = () => _handler.Handle(command, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<CannotUpdateOtherUserProfileException>();
        thrown.Which.AuthenticatedUserId.Should().Be(user.Handle.ToString());
        thrown.Which.TargetUserId.Should().Be(command.UserHandle);
        await _profileRepository.DidNotReceive()
            .UpsertDisplayNameAsync(Arg.Any<UserId>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidTargetUser_ThrowsProfileUpdateForbiddenException()
    {
        var user = CreateUser("alice", "example.com");
        var command = new UpdateDisplayName.Command(user.Id, "alice", "Alice");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CannotUpdateOtherUserProfileException>();
        await _profileRepository.DidNotReceive()
            .UpsertDisplayNameAsync(Arg.Any<UserId>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CurrentUserMissing_ThrowsForbiddenException()
    {
        var userId = UserId.Random();
        var command = new UpdateDisplayName.Command(userId, "@alice:example.com", "Alice");
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _profileRepository.DidNotReceive()
            .UpsertDisplayNameAsync(Arg.Any<UserId>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var user = CreateUser("alice", "example.com");
        var command = new UpdateDisplayName.Command(user.Id, user.Handle.ToString(), "Alice");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(command, cancellationSource.Token);

        await _userRepository.Received().GetByIdAsync(user.Id, cancellationSource.Token);
        await _profileRepository.Received().UpsertDisplayNameAsync(
            Arg.Any<UserId>(),
            Arg.Any<string>(),
            cancellationSource.Token);
    }

    private static User CreateUser(string localpart, string domain) =>
        new(UserId.Random(), new UserHandle(localpart, domain));
}