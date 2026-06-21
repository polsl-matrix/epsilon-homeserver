using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class RegisterAccountTests
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMatrixConfigurationRepository _configurationRepository;
    private readonly IPasswordRepository _passwordRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly ISessionFactory _sessionFactory;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;

    private readonly RegisterAccount.Handler _handler;

    public RegisterAccountTests()
    {
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _configurationRepository = Substitute.For<IMatrixConfigurationRepository>();
        _passwordRepository = Substitute.For<IPasswordRepository>();
        _profileRepository = Substitute.For<IProfileRepository>();
        _sessionFactory = Substitute.For<ISessionFactory>();
        _sessionRepository = Substitute.For<ISessionRepository>();
        _userRepository = Substitute.For<IUserRepository>();

        _handler = new RegisterAccount.Handler(
            _passwordHasher,
            _configurationRepository,
            _passwordRepository,
            _profileRepository,
            _sessionFactory,
            _sessionRepository,
            _userRepository);
    }

    [Fact]
    public async Task Handle_ValidRegistration_ReturnsUserHandleAndTokens()
    {
        var command = new RegisterAccount.Command("alice", "s3cur3*pass!");

        var domain = new Domain.Common.Values.Domain("wonderland.net");
        var session = new Session(SessionId.Random(), UserId.Random(), "hash-accessToken#3"u8.ToArray(), "hash-refreshToken$4"u8.ToArray());

        _configurationRepository.GetDomainAsync(Arg.Any<CancellationToken>()).Returns(domain);
        _userRepository.GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _passwordHasher.HashAsync(command.Password, Arg.Any<CancellationToken>()).Returns("hash-password%5"u8.ToArray());
        _sessionFactory.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns((session, "mock-accessToken!1", "mock-refreshToken@2"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Handle.Localpart.Value.Should().Be(command.Username);
        result.Handle.Domain.Value.Should().Be(domain.Value);
        result.AccessToken.Should().Be("mock-accessToken!1");
        result.RefreshToken.Should().Be("mock-refreshToken@2");
    }

    [Fact]
    public async Task Handle_UsernameTaken_ThrowsUsernameTakenException()
    {
        var command = new RegisterAccount.Command("bob", "password#99");

        var domain = new Domain.Common.Values.Domain("matrix.org");
        var existingUser = new User(UserId.Random(), new UserHandle("bob", domain.Value));

        _configurationRepository.GetDomainAsync(Arg.Any<CancellationToken>()).Returns(domain);
        _userRepository.GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>()).Returns(existingUser);

        var act = () => _handler.Handle(command, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<UsernameTakenException>();
        thrown.Which.Message.Should().Contain(command.Username);
        await _userRepository.DidNotReceive().InsertAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();

        var command = new RegisterAccount.Command("carol", "my_safe_pass1");
        var domain = new Domain.Common.Values.Domain("test.server");
        var session = new Session(SessionId.Random(), UserId.Random(), [], []);

        _configurationRepository.GetDomainAsync(Arg.Any<CancellationToken>()).Returns(domain);
        _userRepository.GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _passwordHasher.HashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns([]);
        _sessionFactory.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns((session, "tok-access", "tok-refresh"));

        await _handler.Handle(command, cancellationSource.Token);

        await _configurationRepository.Received().GetDomainAsync(cancellationSource.Token);
        await _userRepository.Received().GetByHandleAsync(Arg.Any<UserHandle>(), cancellationSource.Token);
        await _passwordHasher.Received().HashAsync(Arg.Any<string>(), cancellationSource.Token);
        await _userRepository.Received().InsertAsync(Arg.Any<User>(), cancellationSource.Token);
        await _profileRepository.Received().InsertAsync(Arg.Any<Profile>(), cancellationSource.Token);
        await _passwordRepository.Received().InsertAsync(Arg.Any<Password>(), cancellationSource.Token);
        await _sessionFactory.Received().CreateAsync(Arg.Any<User>(), cancellationSource.Token);
        await _sessionRepository.Received().UpsertAsync(Arg.Any<Session>(), cancellationSource.Token);
    }
}