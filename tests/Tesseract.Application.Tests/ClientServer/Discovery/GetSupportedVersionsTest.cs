using FluentAssertions;
using NSubstitute;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Discovery.Abstractions;

namespace Tesseract.Application.Tests.ClientServer.Discovery;

public class GetSupportedVersionsTest
{
    private readonly IVersionRepository _repository;
    private readonly GetSupportedVersions.Handler _handler;

    public GetSupportedVersionsTest()
    {
        _repository = Substitute.For<IVersionRepository>();
        _handler = new GetSupportedVersions.Handler(_repository);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsRepositoryWithCancellationToken()
    {
        var cancellationSource = new CancellationTokenSource();

        await _handler.Handle(new GetSupportedVersions.Query(), cancellationSource.Token);

        await _repository.Received(1).GetSupportedVersions(cancellationSource.Token);
    }

    [Theory]
    [InlineData((object)new string[] { })]
    [InlineData((object)new[] { "r0.0.1", "v1.1", "m.inv" })]
    public async Task Handle_ValidRequest_ReturnsResponseWithIdenticalVersions(string[] versions)
    {
        _repository.GetSupportedVersions(Arg.Any<CancellationToken>()).Returns(versions);

        var response = await _handler.Handle(new GetSupportedVersions.Query(), CancellationToken.None);

        response.Versions.Should().Equal(versions);
    }
}