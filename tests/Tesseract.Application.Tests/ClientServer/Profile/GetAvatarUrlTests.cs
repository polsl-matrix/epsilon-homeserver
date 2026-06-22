using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Profile;
using Tesseract.Application.ClientServer.Profile.Exceptions;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Profile;

public sealed class GetAvatarUrlTests
{
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly GetAvatarUrl.Handler _handler;

    public GetAvatarUrlTests()
    {
        _handler = new GetAvatarUrl.Handler(_profileRepository);
    }

    [Fact]
    public async Task Handle_ProfileHasAvatarUrl_ReturnsAvatarUrl()
    {
        var query = new GetAvatarUrl.Query("@alice:example.com");
        _profileRepository.GetAvatarUrlAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns("mxc://example.com/avatar");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.AvatarUrl.Should().Be("mxc://example.com/avatar");
    }

    [Fact]
    public async Task Handle_ProfileHasEmptyAvatarUrl_ReturnsEmptyAvatarUrl()
    {
        var query = new GetAvatarUrl.Query("@alice:example.com");
        _profileRepository.GetAvatarUrlAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.AvatarUrl.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ProfileMissingAvatarUrl_ThrowsProfileFieldNotFoundException()
    {
        var query = new GetAvatarUrl.Query("@missing:example.com");
        _profileRepository.GetAvatarUrlAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<ProfileFieldNotFoundException>();
        thrown.Which.UserId.Should().Be(query.UserId);
        thrown.Which.Field.Should().Be("avatar_url");
    }

    [Fact]
    public async Task Handle_InvalidUserId_ThrowsProfileFieldNotFoundExceptionWithoutRepositoryLookup()
    {
        var query = new GetAvatarUrl.Query("not-a-matrix-id");

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ProfileFieldNotFoundException>();
        await _profileRepository.DidNotReceive()
            .GetAvatarUrlAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var query = new GetAvatarUrl.Query("@alice:example.com");
        _profileRepository.GetAvatarUrlAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns("mxc://example.com/avatar");

        await _handler.Handle(query, cancellationSource.Token);

        await _profileRepository.Received()
            .GetAvatarUrlAsync(Arg.Any<UserHandle>(), cancellationSource.Token);
    }

    [Fact]
    public async Task Handle_ValidUserId_PassesParsedHandleToRepository()
    {
        var query = new GetAvatarUrl.Query("@alice:example.com");
        _profileRepository.GetAvatarUrlAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns("mxc://example.com/avatar");

        await _handler.Handle(query, CancellationToken.None);

        await _profileRepository.Received().GetAvatarUrlAsync(
            Arg.Is<UserHandle>(handle =>
                handle.Localpart.Value == "alice" && handle.Domain.Value == "example.com"),
            Arg.Any<CancellationToken>());
    }
}