using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Profile;

public sealed class UpdateAvatarUrlTests
{
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly UpdateAvatarUrl.Handler _handler;

    public UpdateAvatarUrlTests()
    {
        _handler = new UpdateAvatarUrl.Handler(_profileRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_OwnerUpdate_UpsertsAvatarUrl()
    {
        var user = CreateUser("alice", "example.com");
        var command = new UpdateAvatarUrl.Command(user.Id, user.Handle.ToString(), "mxc://example.com/avatar");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        await _profileRepository.Received().UpsertAvatarUrlAsync(
            Arg.Is<UserHandle>(handle => handle == user.Handle),
            "mxc://example.com/avatar",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NullAvatarUrl_UpsertsNullAvatarUrl()
    {
        var user = CreateUser("alice", "example.com");
        var command = new UpdateAvatarUrl.Command(user.Id, user.Handle.ToString(), null);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        await _profileRepository.Received().UpsertAvatarUrlAsync(
            Arg.Any<UserHandle>(),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DifferentTargetUser_ThrowsProfileUpdateForbiddenException()
    {
        var user = CreateUser("alice", "example.com");
        var command = new UpdateAvatarUrl.Command(user.Id, "@bob:example.com", "mxc://example.com/bob");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var act = () => _handler.Handle(command, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<ProfileUpdateForbiddenException>();
        thrown.Which.AuthenticatedUserId.Should().Be(user.Handle.ToString());
        thrown.Which.TargetUserId.Should().Be(command.UserId);
        await _profileRepository.DidNotReceive()
            .UpsertAvatarUrlAsync(Arg.Any<UserHandle>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidTargetUser_ThrowsProfileUpdateForbiddenException()
    {
        var user = CreateUser("alice", "example.com");
        var command = new UpdateAvatarUrl.Command(user.Id, "alice", "mxc://example.com/avatar");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ProfileUpdateForbiddenException>();
        await _profileRepository.DidNotReceive()
            .UpsertAvatarUrlAsync(Arg.Any<UserHandle>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CurrentUserMissing_ThrowsForbiddenException()
    {
        var userId = UserId.Random();
        var command = new UpdateAvatarUrl.Command(userId, "@alice:example.com", "mxc://example.com/avatar");
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _profileRepository.DidNotReceive()
            .UpsertAvatarUrlAsync(Arg.Any<UserHandle>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var user = CreateUser("alice", "example.com");
        var command = new UpdateAvatarUrl.Command(user.Id, user.Handle.ToString(), "mxc://example.com/avatar");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _handler.Handle(command, cancellationSource.Token);

        await _userRepository.Received().GetByIdAsync(user.Id, cancellationSource.Token);
        await _profileRepository.Received().UpsertAvatarUrlAsync(
            Arg.Any<UserHandle>(),
            Arg.Any<string?>(),
            cancellationSource.Token);
    }

    private static User CreateUser(string localpart, string domain) =>
        new(UserId.Random(), new UserHandle(localpart, domain));
}