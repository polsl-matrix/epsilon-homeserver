using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Auth;
using Tesseract.Application.ClientServer.Auth.Abstractions;

namespace Tesseract.Application.Tests.ClientServer.Discovery;

public class GetSupportedAuthenticationFlowsTests
{
    private readonly List<IAuthenticationFlow> _flows;
    private readonly GetSupportedAuthenticationFlows.Handler _handler;

    public GetSupportedAuthenticationFlowsTests()
    {
        _flows = [];
        _handler = new GetSupportedAuthenticationFlows.Handler(_flows);
    }

    [Fact]
    public async Task Handle_FlowsExist_ReturnsResponseWithMappedLoginFlows()
    {
        var a = Substitute.For<IAuthenticationFlow>();
        var b = Substitute.For<IAuthenticationFlow>();
        a.Type.Returns("m.login.password");
        b.Type.Returns("m.login.sso");
        _flows.AddRange([a, b]);

        var response = await _handler.Handle(new GetSupportedAuthenticationFlows.Query(), CancellationToken.None);

        response.Flows.Should().HaveCount(2);
        response.Flows[0].Type.Should().Be("m.login.password");
        response.Flows[1].Type.Should().Be("m.login.sso");
    }

    [Fact]
    public async Task Handle_NoFlowsExist_ReturnsResponseWithEmptyFlowsList()
    {
        var response = await _handler.Handle(new GetSupportedAuthenticationFlows.Query(), CancellationToken.None);

        response.Flows.Should().BeEmpty();
    }
}