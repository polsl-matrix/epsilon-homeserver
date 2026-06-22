using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
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
        var userRepository = Substitute.For<IUserRepository>();

        _handler = new GetDisplayName.Handler(_profileRepository, userRepository);
    }

    [Fact]
    public async Task Handle_DisplayNameExists_ReturnsDisplayName()
    {
        var query = new GetDisplayName.Query("@alice:example.com");
        _profileRepository.GetDisplayNameAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns("Alice");

        var response = await _handler.Handle(query, CancellationToken.None);

        response.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_DisplayNameIsEmpty_ReturnsEmptyDisplayName()
    {
        var query = new GetDisplayName.Query("@alice:example.com");
        _profileRepository.GetDisplayNameAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        var response = await _handler.Handle(query, CancellationToken.None);

        response.DisplayName.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_InvalidUserId_ThrowsProfileFieldNotFoundException()
    {
        var query = new GetDisplayName.Query("alice:example.com");

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ProfileFieldNotFoundException>();
        await _profileRepository.DidNotReceive().GetDisplayNameAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var query = new GetDisplayName.Query("@alice:example.com");
        _profileRepository.GetDisplayNameAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns("Alice");

        await _handler.Handle(query, cancellationSource.Token);

        await _profileRepository.Received().GetDisplayNameAsync(Arg.Any<UserId>(), cancellationSource.Token);
    }
}