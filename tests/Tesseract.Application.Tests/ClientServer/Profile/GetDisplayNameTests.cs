using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Profile;

public class GetDisplayNameTests
{
    private readonly IProfileRepository _profileRepository;
    private readonly GetDisplayName.Handler _handler;

    public GetDisplayNameTests()
    {
        _profileRepository = Substitute.For<IProfileRepository>();
        _handler = new GetDisplayName.Handler(_profileRepository);
    }

    [Fact]
    public async Task Handle_DisplayNameExists_ReturnsDisplayName()
    {
        var query = new GetDisplayName.Query("@alice:example.com");
        _profileRepository.GetDisplayNameAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns("Alice");

        var response = await _handler.Handle(query, CancellationToken.None);

        response.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_DisplayNameIsEmpty_ReturnsEmptyDisplayName()
    {
        var query = new GetDisplayName.Query("@alice:example.com");
        _profileRepository.GetDisplayNameAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        var response = await _handler.Handle(query, CancellationToken.None);

        response.DisplayName.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ProfileFieldMissing_ThrowsProfileFieldNotFoundException()
    {
        var query = new GetDisplayName.Query("@alice:example.com");
        _profileRepository.GetDisplayNameAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<ProfileFieldNotFoundException>();
        thrown.Which.UserId.Should().Be(query.UserId);
        thrown.Which.Field.Should().Be("displayname");
    }

    [Fact]
    public async Task Handle_InvalidUserId_ThrowsProfileFieldNotFoundException()
    {
        var query = new GetDisplayName.Query("alice:example.com");

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ProfileFieldNotFoundException>();
        await _profileRepository.DidNotReceive().GetDisplayNameAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var query = new GetDisplayName.Query("@alice:example.com");
        _profileRepository.GetDisplayNameAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns("Alice");

        await _handler.Handle(query, cancellationSource.Token);

        await _profileRepository.Received().GetDisplayNameAsync(Arg.Any<UserHandle>(), cancellationSource.Token);
    }

    [Fact]
    public async Task Handle_ValidUserId_PassesParsedHandleToRepository()
    {
        var query = new GetDisplayName.Query("@alice:example.com");
        UserHandle? calledHandle = null;
        _profileRepository.GetDisplayNameAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns("Alice")
            .AndDoes(call => calledHandle = call.Arg<UserHandle>());

        await _handler.Handle(query, CancellationToken.None);

        calledHandle.Should().NotBeNull();
        calledHandle!.Localpart.Value.Should().Be("alice");
        calledHandle.Domain.Value.Should().Be("example.com");
    }
}