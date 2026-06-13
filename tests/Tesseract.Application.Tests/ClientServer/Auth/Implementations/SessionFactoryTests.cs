using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Implementations;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.Tests.ClientServer.Auth.Implementations;

public class SessionFactoryTests
{
    private readonly IHashService _hashService;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    private readonly SessionFactory _factory;

    public SessionFactoryTests()
    {
        _hashService = Substitute.For<IHashService>();
        _accessTokenService = Substitute.For<IAccessTokenService>();
        _refreshTokenService = Substitute.For<IRefreshTokenService>();

        _factory = new SessionFactory(_hashService, _accessTokenService, _refreshTokenService);
    }

    [Fact]
    public async Task CreateAsync_ReturnsValidSessionAndTokens()
    {
        var user = new User(UserId.Random(), new UserHandle("toby", "fi.sh"));

        _accessTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-accessToken!1");
        _refreshTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("mock-refreshToken@2");
        _hashService.HashAsync("mock-accessToken!1", Arg.Any<CancellationToken>()).Returns("hash-accessToken#3"u8.ToArray());
        _hashService.HashAsync("mock-refreshToken@2", Arg.Any<CancellationToken>()).Returns("hash-refreshToken$4"u8.ToArray());

        var (session, accessToken, refreshToken) = await _factory.CreateAsync(user, CancellationToken.None);

        session.UserId.Value.Should().Be(user.Id.Value);
        session.AccessTokenHash.Should().BeEqualTo("hash-accessToken#3"u8.ToArray());
        session.RefreshTokenHash.Should().BeEqualTo("hash-refreshToken$4"u8.ToArray());
        accessToken.Should().Be("mock-accessToken!1");
        refreshToken.Should().Be("mock-refreshToken@2");
    }

    [Fact]
    public async Task CreateAsync_GeneratesUniqueSessionIds()
    {
        var user = new User(UserId.Random(), new UserHandle("tom", "the.cat"));

        var (a, _, _) = await _factory.CreateAsync(user, CancellationToken.None);
        var (b, _, _) = await _factory.CreateAsync(user, CancellationToken.None);

        a.Id.Value.Should().NotBe(b.Id.Value);
    }

    [Fact]
    public async Task CreateAsync_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();

        var user = new User(UserId.Random(), new UserHandle("hairy", "be.ar"));
        _accessTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("test-accessToken!1");
        _refreshTokenService.CreateAsync(Arg.Any<CancellationToken>()).Returns("test-refreshToken@2");
        _hashService.HashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns([]);

        await _factory.CreateAsync(user, cancellationSource.Token);

        await _accessTokenService.Received().CreateAsync(cancellationSource.Token);
        await _refreshTokenService.Received().CreateAsync(cancellationSource.Token);
        await _hashService.Received().HashAsync("test-accessToken!1", cancellationSource.Token);
        await _hashService.Received().HashAsync("test-refreshToken@2", cancellationSource.Token);
    }
}