using Tesseract.Application.ClientServer.Auth.Models;
using Tesseract.Domain.Users;
using Tesseract.Infrastructure.ClientServer.Auth.Dao;

namespace Tesseract.Infrastructure.ClientServer.Auth.Mappers;

internal static class PasswordMapper
{
    public static Password ToDomain(this PasswordDao dao)
    {
        var userId = new UserId(dao.UserId);

        return new Password(userId, dao.PasswordHash);
    }
}