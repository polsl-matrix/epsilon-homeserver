using FluentAssertions;
using Tesseract.Infrastructure.ClientServer.Auth.Dao;
using Tesseract.Infrastructure.ClientServer.Auth.Mappers;

namespace Tesseract.Infrastructure.Tests.ClientServer.Auth.Mappers;

public class SessionMapperTests
{
    [Fact]
    public void ToDomain_Always_MapsCurrentTokenHashes()
    {
        var dao = new SessionDao
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CurrentAccessTokenHash = "current-access"u8.ToArray(),
            CurrentRefreshTokenHash = "current-refresh"u8.ToArray(),
        };

        var session = dao.ToDomain();

        session.CurrentAccessTokenHash.Should().BeEqualTo("current-access"u8.ToArray());
        session.CurrentRefreshTokenHash.Should().BeEqualTo("current-refresh"u8.ToArray());
    }

    [Fact]
    public void ToDomain_NullPendingTokenHashes_MapsNullPendingTokenHashes()
    {
        var dao = new SessionDao
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CurrentAccessTokenHash = "current-access"u8.ToArray(),
            CurrentRefreshTokenHash = "current-refresh"u8.ToArray(),
            PendingAccessTokenHash = null,
            PendingRefreshTokenHash = null,
        };

        var session = dao.ToDomain();

        session.PendingAccessTokenHash.Should().BeNull();
        session.PendingRefreshTokenHash.Should().BeNull();
    }

    [Fact]
    public void ToDomain_PendingTokenHashes_MapsPendingTokenHashes()
    {
        var dao = new SessionDao
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CurrentAccessTokenHash = "current-access"u8.ToArray(),
            CurrentRefreshTokenHash = "current-refresh"u8.ToArray(),
            PendingAccessTokenHash = "pending-access"u8.ToArray(),
            PendingRefreshTokenHash = "pending-refresh"u8.ToArray(),
        };

        var session = dao.ToDomain();

        session.PendingAccessTokenHash.Should().BeEqualTo("pending-access"u8.ToArray());
        session.PendingRefreshTokenHash.Should().BeEqualTo("pending-refresh"u8.ToArray());
    }
}