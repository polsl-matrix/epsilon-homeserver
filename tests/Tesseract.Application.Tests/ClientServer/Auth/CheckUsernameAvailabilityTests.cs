using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Exceptions;
using Tesseract.Application.ClientServer.Identity.Abstractions;
using Tesseract.Application.Common.Configuration;
using Tesseract.Domain.Users;

namespace Tesseract.Application.Tests.ClientServer.Auth;

public class CheckUsernameAvailabilityTests
{
    private readonly IMatrixConfigurationRepository _configurationRepository;
    private readonly IUserRepository _userRepository;
    private readonly CheckUsernameAvailability.Handler _handler;

    public CheckUsernameAvailabilityTests()
    {
        _configurationRepository = Substitute.For<IMatrixConfigurationRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new CheckUsernameAvailability.Handler(_configurationRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_AvailableUsername_ReturnsAvailableTrue()
    {
        var domain = new Domain.Common.Values.Domain("example.com");
        _configurationRepository.GetDomainAsync(Arg.Any<CancellationToken>()).Returns(domain);
        _userRepository.GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(new CheckUsernameAvailability.Query("alice"), CancellationToken.None);

        result.Available.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MissingUsername_ThrowsInvalidUsernameException()
    {
        var act = () => _handler.Handle(new CheckUsernameAvailability.Query(null), CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidUsernameException>();
        thrown.Which.Username.Should().BeNull();
        await _configurationRepository.DidNotReceive().GetDomainAsync(Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidUsername_ThrowsInvalidUsernameException()
    {
        var act = () => _handler.Handle(new CheckUsernameAvailability.Query("Alice"), CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidUsernameException>();
        thrown.Which.Username.Should().Be("Alice");
        await _userRepository.DidNotReceive().GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TakenUsername_ThrowsUsernameTakenException()
    {
        var domain = new Domain.Common.Values.Domain("example.com");
        var existingUser = new User(UserId.Random(), new UserHandle("bob", domain.Value));
        _configurationRepository.GetDomainAsync(Arg.Any<CancellationToken>()).Returns(domain);
        _userRepository.GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>()).Returns(existingUser);

        var act = () => _handler.Handle(new CheckUsernameAvailability.Query("bob"), CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<UsernameTakenException>();
        thrown.Which.UserId.Should().Be("bob");
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenDown()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var domain = new Domain.Common.Values.Domain("example.com");
        _configurationRepository.GetDomainAsync(Arg.Any<CancellationToken>()).Returns(domain);
        _userRepository.GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        await _handler.Handle(new CheckUsernameAvailability.Query("carol"), cancellationToken);

        await _configurationRepository.Received().GetDomainAsync(cancellationToken);
        await _userRepository.Received().GetByHandleAsync(Arg.Any<UserHandle>(), cancellationToken);
    }

    [Fact]
    public async Task Handle_ValidUsername_PassesHomeserverHandleToRepository()
    {
        var domain = new Domain.Common.Values.Domain("matrix.example");
        _configurationRepository.GetDomainAsync(Arg.Any<CancellationToken>()).Returns(domain);
        _userRepository.GetByHandleAsync(Arg.Any<UserHandle>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        await _handler.Handle(new CheckUsernameAvailability.Query("dave"), CancellationToken.None);

        await _userRepository.Received().GetByHandleAsync(
            Arg.Is<UserHandle>(handle => handle.Localpart.Value == "dave" && handle.Domain.Value == domain.Value),
            Arg.Any<CancellationToken>());
    }
}