using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;
using Tesseract.Infrastructure.ClientServer.Auth.Dao;

namespace Tesseract.Infrastructure.ClientServer.Auth.Mappers;

internal static class SessionMapper
{
    public static Session ToDomain(this SessionDao dao)
    {
        var sessionId = new SessionId(dao.SessionId);
        var userId = new UserId(dao.UserId);

        return new Session(sessionId, userId, dao.CurrentAccessTokenHash, dao.CurrentRefreshTokenHash, dao.DeviceId);
    }
}