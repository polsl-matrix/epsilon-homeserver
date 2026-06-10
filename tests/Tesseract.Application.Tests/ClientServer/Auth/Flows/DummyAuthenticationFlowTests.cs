using FluentAssertions;
using Tesseract.Application.ClientServer.Auth.Flows;

namespace Tesseract.Application.Tests.ClientServer.Auth.Flows;

public class DummyAuthenticationFlowTests
{
    private readonly DummyAuthenticationFlow _flow = new();

    [Fact]
    public void Type_ReturnsDummyAuthentication()
    {
        var type = _flow.Type;

        type.Should().Be("m.login.dummy");
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsDummyUser()
    {
        var user = await _flow.AuthenticateAsync(string.Empty, string.Empty, CancellationToken.None);

        user.Should().NotBeNull();
        user.Id.Should().Be(Guid.Empty);
        user.Handle.ToString().Should().ContainEquivalentOf("dummy");
    }
}