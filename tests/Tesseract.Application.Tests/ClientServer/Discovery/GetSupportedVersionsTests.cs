using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tesseract.Application.ClientServer.Discovery;
using Tesseract.Application.ClientServer.Discovery.Abstractions;

namespace Tesseract.Application.Tests.ClientServer.Discovery;

public class GetSupportedVersionsTests
{
    private readonly IVersionRepository _repository;
    private readonly GetSupportedVersions.Handler _handler;

    public GetSupportedVersionsTests()
    {
        _repository = Substitute.For<IVersionRepository>();
        _handler = new GetSupportedVersions.Handler(_repository);
    }

    [Theory]
    [InlineData((object)new string[] { })]
    [InlineData((object)new[] { "r0.0.1", "v1.1", "v.1.18-alpha" })]
    public async Task Handle_RepositoryReturnsVersions_ReturnsResponseWithIdenticalVersions(string[] versions)
    {
        _repository.GetSupportedVersionsAsync(Arg.Any<CancellationToken>())
            .Returns(versions);

        var response = await _handler.Handle(new GetSupportedVersions.Query(), CancellationToken.None);

        response.Versions.Should().Equal(versions);
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesSameTokenToRepository()
    {
        var cancellationSource = new CancellationTokenSource();

        await _handler.Handle(new GetSupportedVersions.Query(), cancellationSource.Token);

        await _repository.Received().GetSupportedVersionsAsync(cancellationSource.Token);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ForwardsSameException()
    {
        var exception = new InvalidOperationException("Something went wrong.");

        _repository.GetSupportedVersionsAsync(Arg.Any<CancellationToken>())
            .Throws(exception);

        var act = () => _handler.Handle(new GetSupportedVersions.Query(), CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(exception);
    }
}