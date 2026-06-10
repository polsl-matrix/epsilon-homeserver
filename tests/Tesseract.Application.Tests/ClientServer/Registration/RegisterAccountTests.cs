using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Registration;
using Tesseract.Application.ClientServer.Registration.Abstractions;
using Tesseract.Application.ClientServer.Registration.Exceptions;
using Tesseract.Domain.Accounts.Values;

namespace Tesseract.Application.Tests.ClientServer.Registration;

public class RegisterAccountTests
{
    private const string ServerName = "example.org";

    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentifierGenerator _identifierGenerator;
    private readonly RegisterAccount.Handler _handler;

    public RegisterAccountTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _identifierGenerator = Substitute.For<IIdentifierGenerator>();

        _passwordHasher.Hash(Arg.Any<string>()).Returns(callInfo => $"hashed:{callInfo.Arg<string>()}");
        _identifierGenerator.GenerateLocalpart().Returns("generatedlocalpart");
        _identifierGenerator.GenerateDeviceId().Returns("GENERATEDID");
        _identifierGenerator.GenerateAccessToken().Returns("generated-access-token");

        _handler = new RegisterAccount.Handler(
            _accountRepository,
            _passwordHasher,
            _identifierGenerator,
            Options.Create(new MatrixOptions { ServerName = ServerName }));
    }

    private static RegisterAccount.Command CreateCommand(
        string kind = "user",
        string? username = "cheeky_monkey",
        string? password = "ilovebananas",
        string? deviceId = "GHTYAJCE",
        string? initialDeviceDisplayName = "Jungle Phone",
        bool inhibitLogin = false,
        RegisterAccount.AuthenticationData? auth = null) =>
        new(kind, username, password, deviceId, initialDeviceDisplayName, inhibitLogin,
            auth ?? new RegisterAccount.AuthenticationData(AuthenticationTypes.Dummy, "session-id"));

    [Theory]
    [InlineData("guest")]
    [InlineData("bot")]
    public async Task Handle_NonUserKind_ThrowsRegistrationNotAllowedException(string kind)
    {
        var act = () => _handler.Handle(CreateCommand(kind: kind), CancellationToken.None);

        await act.Should().ThrowAsync<RegistrationNotAllowedException>();
    }

    [Fact]
    public async Task Handle_MissingServerName_ThrowsInvalidOperationException()
    {
        var handler = new RegisterAccount.Handler(
            _accountRepository, _passwordHasher, _identifierGenerator,
            Options.Create(new MatrixOptions()));

        var act = () => handler.Handle(CreateCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("CheekyMonkey")]
    [InlineData("cheeky monkey")]
    [InlineData("chęeky")]
    [InlineData("cheeky!")]
    public async Task Handle_InvalidUsername_ThrowsInvalidUsernameException(string username)
    {
        var act = () => _handler.Handle(CreateCommand(username: username), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidUsernameException>();
    }

    [Fact]
    public async Task Handle_UsernameMakingUserIdTooLong_ThrowsInvalidUsernameException()
    {
        var username = new string('a', 255);

        var act = () => _handler.Handle(CreateCommand(username: username), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidUsernameException>();
    }

    [Fact]
    public async Task Handle_UsernameTaken_ThrowsUserInUseException()
    {
        _accountRepository.IsLocalpartTaken("cheeky_monkey", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<UserInUseException>();
    }

    [Fact]
    public async Task Handle_NoAuth_ReturnsAuthenticationRequiredWithDummyFlow()
    {
        var response = await _handler.Handle(CreateCommand() with { Auth = null }, CancellationToken.None);

        var authenticationRequired = response.Should()
            .BeOfType<RegisterAccount.Response.AuthenticationRequired>().Subject;
        authenticationRequired.Session.Should().NotBeNullOrWhiteSpace();
        authenticationRequired.Flows.Should().ContainSingle()
            .Which.Should().Equal(AuthenticationTypes.Dummy);
        await _accountRepository.DidNotReceive()
            .CreateAccount(Arg.Any<Account>(), Arg.Any<Device?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnsupportedAuthType_ReturnsAuthenticationRequiredWithSameSession()
    {
        var auth = new RegisterAccount.AuthenticationData("m.login.password", "existing-session");

        var response = await _handler.Handle(CreateCommand(auth: auth), CancellationToken.None);

        var authenticationRequired = response.Should()
            .BeOfType<RegisterAccount.Response.AuthenticationRequired>().Subject;
        authenticationRequired.Session.Should().Be("existing-session");
    }

    [Fact]
    public async Task Handle_DummyAuth_RegistersAccountWithDevice()
    {
        var response = await _handler.Handle(CreateCommand(), CancellationToken.None);

        await _accountRepository.Received().CreateAccount(
            new Account("cheeky_monkey", "hashed:ilovebananas"),
            new Device("GHTYAJCE", "Jungle Phone", "generated-access-token"),
            Arg.Any<CancellationToken>());

        var registered = response.Should().BeOfType<RegisterAccount.Response.Registered>().Subject;
        registered.UserId.Should().Be($"@cheeky_monkey:{ServerName}");
        registered.DeviceId.Should().Be("GHTYAJCE");
        registered.AccessToken.Should().Be("generated-access-token");
    }

    [Fact]
    public async Task Handle_NoUsername_GeneratesLocalpart()
    {
        var response = await _handler.Handle(CreateCommand(username: null), CancellationToken.None);

        var registered = response.Should().BeOfType<RegisterAccount.Response.Registered>().Subject;
        registered.UserId.Should().Be($"@generatedlocalpart:{ServerName}");
    }

    [Fact]
    public async Task Handle_NoDeviceId_GeneratesDeviceId()
    {
        var response = await _handler.Handle(CreateCommand(deviceId: null), CancellationToken.None);

        var registered = response.Should().BeOfType<RegisterAccount.Response.Registered>().Subject;
        registered.DeviceId.Should().Be("GENERATEDID");
    }

    [Fact]
    public async Task Handle_NoPassword_StoresAccountWithoutPasswordHash()
    {
        await _handler.Handle(CreateCommand(password: null), CancellationToken.None);

        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
        await _accountRepository.Received().CreateAccount(
            new Account("cheeky_monkey", null), Arg.Any<Device?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InhibitLogin_RegistersAccountWithoutDevice()
    {
        var response = await _handler.Handle(CreateCommand(inhibitLogin: true), CancellationToken.None);

        await _accountRepository.Received().CreateAccount(
            Arg.Any<Account>(), null, Arg.Any<CancellationToken>());

        var registered = response.Should().BeOfType<RegisterAccount.Response.Registered>().Subject;
        registered.DeviceId.Should().BeNull();
        registered.AccessToken.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenToRepository()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        await _handler.Handle(CreateCommand(), cancellationToken);

        await _accountRepository.Received().IsLocalpartTaken("cheeky_monkey", cancellationToken);
        await _accountRepository.Received().CreateAccount(
            Arg.Any<Account>(), Arg.Any<Device?>(), cancellationToken);
    }
}