using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class RegisterAccountTests
{
    private readonly IMatrixConfigurationRepository _matrixConfigurationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionFactory _sessionFactory;
    private readonly IAccountRegistrationRepository _accountRegistrationRepository;
    private readonly ILocalpartGenerator _localpartGenerator;
    private readonly IDeviceIdGenerator _deviceIdGenerator;
    private readonly RegisterAccount.Handler _handler;

    public RegisterAccountTests()
    {
        _matrixConfigurationRepository = Substitute.For<IMatrixConfigurationRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _sessionFactory = Substitute.For<ISessionFactory>();
        _accountRegistrationRepository = Substitute.For<IAccountRegistrationRepository>();
        _localpartGenerator = Substitute.For<ILocalpartGenerator>();
        _deviceIdGenerator = Substitute.For<IDeviceIdGenerator>();

        _matrixConfigurationRepository.GetDomainAsync(Arg.Any<CancellationToken>())
            .Returns(new Tesseract.Domain.Users.Values.Domain("example.com"));
        _passwordHasher.HashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("hashed-password"u8.ToArray());
        _deviceIdGenerator.Create().Returns("DEVICEID");
        _localpartGenerator.Create().Returns(new Localpart("generated"));

        _handler = new RegisterAccount.Handler(
            _matrixConfigurationRepository,
            _userRepository,
            _passwordHasher,
            _sessionFactory,
            _accountRegistrationRepository,
            _localpartGenerator,
            _deviceIdGenerator);
    }

    [Fact]
    public async Task Handle_ValidDummyAuth_CreatesAccountAndReturnsLoginData()
    {
        var command = CreateCommand(deviceId: "ABCDEF", refreshToken: true);

        var session = new Session(SessionId.Random(), UserId.Random(), "access-hash"u8.ToArray(), "refresh-hash"u8.ToArray());
        _sessionFactory.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns((session, "access-token", "refresh-token"));

        AccountRegistration? registration = null;
        _accountRegistrationRepository.CreateAsync(Arg.Any<AccountRegistration>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(call => registration = call.Arg<AccountRegistration>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Handle.ToString().Should().Be("@alice:example.com");
        result.AccessToken.Should().Be("access-token");
        result.DeviceId.Should().Be("ABCDEF");
        result.RefreshToken.Should().Be("refresh-token");

        registration.Should().NotBeNull();
        registration.User.Handle.ToString().Should().Be("@alice:example.com");
        registration.Profile.UserId.Should().Be(registration.User.Id);
        registration.PasswordHash.Should().BeEquivalentTo("hashed-password"u8.ToArray());
        registration.Device.Should().NotBeNull();
        registration.Device!.Id.Should().Be("ABCDEF");
        registration.Device.DisplayName.Should().Be("Alice's Phone");
        registration.Session.Should().NotBeNull();
        registration.Session!.DeviceId.Should().Be("ABCDEF");
    }

    [Fact]
    public async Task Handle_DeviceIdOmitted_GeneratesDeviceId()
    {
        var command = CreateCommand(deviceId: null);
        var session = new Session(SessionId.Random(), UserId.Random(), [], []);
        _sessionFactory.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns((session, "access", "refresh"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.DeviceId.Should().Be("DEVICEID");
        _deviceIdGenerator.Received().Create();
    }

    [Fact]
    public async Task Handle_UsernameOmitted_GeneratesLocalpart()
    {
        var command = CreateCommand(username: null);
        var session = new Session(SessionId.Random(), UserId.Random(), [], []);
        _sessionFactory.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns((session, "access", "refresh"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Handle.ToString().Should().Be("@generated:example.com");
        _localpartGenerator.Received().Create();
    }

    [Fact]
    public async Task Handle_InhibitLogin_DoesNotCreateDeviceOrSession()
    {
        var command = CreateCommand(inhibitLogin: true);

        AccountRegistration? registration = null;
        _accountRegistrationRepository.CreateAsync(Arg.Any<AccountRegistration>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(call => registration = call.Arg<AccountRegistration>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().BeNull();
        result.DeviceId.Should().BeNull();
        result.RefreshToken.Should().BeNull();
        registration!.Device.Should().BeNull();
        registration.Session.Should().BeNull();
        await _sessionFactory.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RefreshTokenNotRequested_OmitsRefreshTokenFromResponse()
    {
        var command = CreateCommand(refreshToken: false);
        var session = new Session(SessionId.Random(), UserId.Random(), [], []);
        _sessionFactory.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns((session, "access", "refresh"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NoAuth_ThrowsUserInteractiveAuthenticationRequiredException()
    {
        var command = CreateCommand(includeAuth: false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<UserInteractiveAuthenticationRequiredException>();
        thrown.Which.Flows.Should().ContainSingle()
            .Which.Should().ContainSingle().Which.Should().Be("m.login.dummy");
        thrown.Which.Session.Should().NotBeNullOrWhiteSpace();
        await _accountRegistrationRepository.DidNotReceive()
            .CreateAsync(Arg.Any<AccountRegistration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnsupportedAuthType_ThrowsUserInteractiveAuthenticationRequiredExceptionWithForbiddenError()
    {
        var command = CreateCommand(auth: new RegisterAccount.AuthenticationData("m.login.password", "session"));

        var act = () => _handler.Handle(command, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<UserInteractiveAuthenticationRequiredException>();
        thrown.Which.ErrorCode.Should().Be("M_FORBIDDEN");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_DummyAuthWithoutSession_ThrowsUserInteractiveAuthenticationRequiredException(string? session)
    {
        var command = CreateCommand(auth: new RegisterAccount.AuthenticationData("m.login.dummy", session));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserInteractiveAuthenticationRequiredException>();
        await _accountRegistrationRepository.DidNotReceive()
            .CreateAsync(Arg.Any<AccountRegistration>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("UPPER")]
    [InlineData("space here")]
    [InlineData("")]
    public async Task Handle_InvalidUsername_ThrowsInvalidUsernameException(string username)
    {
        var command = CreateCommand(username: username);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidUsernameException>();
        await _accountRegistrationRepository.DidNotReceive()
            .CreateAsync(Arg.Any<AccountRegistration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserAlreadyExists_ThrowsUserInUseExceptionBeforeAuthentication()
    {
        var command = CreateCommand(includeAuth: false);
        _userRepository.GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>())
            .Returns(new User(UserId.Random(), new UserHandle("alice", "example.com")));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserInUseException>();
    }

    [Fact]
    public async Task Handle_PasswordOmitted_ThrowsMissingParameterException()
    {
        var command = CreateCommand(password: null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<MissingParameterException>();
        thrown.Which.ParameterName.Should().Be("password");
    }

    [Fact]
    public async Task Handle_UnsupportedKind_ThrowsRegistrationForbiddenException()
    {
        var command = CreateCommand(kind: "guest");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RegistrationForbiddenException>();
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationSource = new CancellationTokenSource();
        var command = CreateCommand();
        var session = new Session(SessionId.Random(), UserId.Random(), [], []);
        _sessionFactory.CreateAsync(Arg.Any<User>(), cancellationSource.Token).Returns((session, "access", "refresh"));

        await _handler.Handle(command, cancellationSource.Token);

        await _matrixConfigurationRepository.Received().GetDomainAsync(cancellationSource.Token);
        await _userRepository.Received(2).GetByHandleAsync(Arg.Any<UserHandle>(), cancellationSource.Token);
        await _passwordHasher.Received().HashAsync(command.Password!, cancellationSource.Token);
        await _sessionFactory.Received().CreateAsync(Arg.Any<User>(), cancellationSource.Token);
        await _accountRegistrationRepository.Received()
            .CreateAsync(Arg.Any<AccountRegistration>(), cancellationSource.Token);
    }

    private static RegisterAccount.Command CreateCommand(
        string? kind = null,
        string? username = "alice",
        string? password = "correct horse battery staple",
        RegisterAccount.AuthenticationData? auth = null,
        bool includeAuth = true,
        string? deviceId = "DEVICE",
        bool inhibitLogin = false,
        bool refreshToken = true) => new(
        kind,
        username,
        password,
        includeAuth ? auth ?? new RegisterAccount.AuthenticationData("m.login.dummy", "session") : null,
        deviceId,
        "Alice's Phone",
        inhibitLogin,
        refreshToken);
}