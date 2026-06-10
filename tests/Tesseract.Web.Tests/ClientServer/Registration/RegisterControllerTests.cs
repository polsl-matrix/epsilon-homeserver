using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Registration;
using Tesseract.Web.ClientServer.Registration;
using Tesseract.Web.ClientServer.Registration.Contracts;

namespace Tesseract.Web.Tests.ClientServer.Registration;

public class RegisterControllerTests
{
    private readonly IMediator _mediator;
    private readonly RegisterController _controller;

    public RegisterControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _controller = new RegisterController(_mediator);
    }

    [Fact]
    public async Task Register_MediatorReturnsRegistered_ReturnsOkWithMappedResponse()
    {
        _mediator.Send(Arg.Any<RegisterAccount.Command>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterAccount.Response.Registered(
                "@cheeky_monkey:example.org", "GHTYAJCE", "abc123"));

        var result = await _controller.Register(new RegisterRequest(), cancellationToken: CancellationToken.None);

        var response = result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<RegisterResponse>().Subject;
        response.UserId.Should().Be("@cheeky_monkey:example.org");
        response.DeviceId.Should().Be("GHTYAJCE");
        response.AccessToken.Should().Be("abc123");
    }

    [Fact]
    public async Task Register_MediatorReturnsAuthenticationRequired_ReturnsUnauthorizedWithFlows()
    {
        _mediator.Send(Arg.Any<RegisterAccount.Command>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterAccount.Response.AuthenticationRequired(
                "session-id", [[AuthenticationTypes.Dummy]]));

        var result = await _controller.Register(new RegisterRequest(), cancellationToken: CancellationToken.None);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var response = objectResult.Value.Should().BeOfType<UserInteractiveAuthResponse>().Subject;
        response.Session.Should().Be("session-id");
        response.Flows.Should().ContainSingle()
            .Which.Stages.Should().Equal(AuthenticationTypes.Dummy);
    }

    [Fact]
    public async Task Register_RequestProvided_MapsRequestToCommand()
    {
        RegisterAccount.Command? command = null;
        _mediator.Send(Arg.Do<RegisterAccount.Command>(c => command = c), Arg.Any<CancellationToken>())
            .Returns(new RegisterAccount.Response.Registered("@user:example.org", null, null));

        var request = new RegisterRequest
        {
            Auth = new RegisterRequest.AuthenticationData
            {
                Type = AuthenticationTypes.Dummy,
                Session = "session-id",
            },
            DeviceId = "GHTYAJCE",
            InhibitLogin = true,
            InitialDeviceDisplayName = "Jungle Phone",
            Password = "ilovebananas",
            Username = "cheeky_monkey",
        };

        await _controller.Register(request, "guest", CancellationToken.None);

        command.Should().Be(new RegisterAccount.Command(
            "guest",
            "cheeky_monkey",
            "ilovebananas",
            "GHTYAJCE",
            "Jungle Phone",
            true,
            new RegisterAccount.AuthenticationData(AuthenticationTypes.Dummy, "session-id")));
    }

    [Fact]
    public async Task Register_NoAuthInRequest_MapsNullAuthToCommand()
    {
        RegisterAccount.Command? command = null;
        _mediator.Send(Arg.Do<RegisterAccount.Command>(c => command = c), Arg.Any<CancellationToken>())
            .Returns(new RegisterAccount.Response.Registered("@user:example.org", null, null));

        await _controller.Register(new RegisterRequest(), cancellationToken: CancellationToken.None);

        command.Should().NotBeNull();
        command.Auth.Should().BeNull();
        command.Kind.Should().Be("user");
    }

    [Fact]
    public async Task Register_CancellationTokenProvided_PassesSameTokenToMediator()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        _mediator.Send(Arg.Any<RegisterAccount.Command>(), cancellationToken)
            .Returns(new RegisterAccount.Response.Registered("@user:example.org", null, null));

        await _controller.Register(new RegisterRequest(), cancellationToken: cancellationToken);

        await _mediator.Received().Send(Arg.Any<RegisterAccount.Command>(), cancellationToken);
    }

    [Fact]
    public async Task Register_MediatorThrowsException_PropagatesException()
    {
        var exception = new InvalidOperationException("Something went wrong.");

        _mediator.Send(Arg.Any<RegisterAccount.Command>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var act = () => _controller.Register(new RegisterRequest(), cancellationToken: CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(exception);
    }
}