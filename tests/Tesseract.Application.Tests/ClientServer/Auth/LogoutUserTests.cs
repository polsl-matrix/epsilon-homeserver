using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class LogoutUserTests
{
    private readonly IHashService _hashService;
    private readonly ISessionRepository _sessionRepository;
    private readonly LogoutUser.Handler _handler;

    public LogoutUserTests()
    {
        _hashService = Substitute.For<IHashService>();
        _sessionRepository = Substitute.For<ISessionRepository>();
        _handler = new LogoutUser.Handler(_hashService, _sessionRepository);
    }

    [Fact]
    public async Task Handle_AccessTokenProvided_DeletesSessionByHashedAccessToken()
    {
        var command = new LogoutUser.Command("mock-accessToken!1");
        var accessTokenHash = "hash-accessToken@2"u8.ToArray();
        _hashService.HashAsync(command.AccessToken, Arg.Any<CancellationToken>()).Returns(accessTokenHash);

        await _handler.Handle(command, CancellationToken.None);

        await _sessionRepository.Received().DeleteByAccessTokenAsync(accessTokenHash, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var command = new LogoutUser.Command("mock-accessToken!1");
        var accessTokenHash = "hash-accessToken@2"u8.ToArray();
        _hashService.HashAsync(command.AccessToken, Arg.Any<CancellationToken>()).Returns(accessTokenHash);

        await _handler.Handle(command, cancellationSource.Token);

        await _hashService.Received().HashAsync(command.AccessToken, cancellationSource.Token);
        await _sessionRepository.Received().DeleteByAccessTokenAsync(accessTokenHash, cancellationSource.Token);
    }
}