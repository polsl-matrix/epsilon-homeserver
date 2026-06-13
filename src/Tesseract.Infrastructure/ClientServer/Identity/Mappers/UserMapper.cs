using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;
using Tesseract.Infrastructure.ClientServer.Identity.Dao;

namespace Tesseract.Infrastructure.ClientServer.Identity.Mappers;

internal static class UserMapper
{
    public static User ToDomain(this UserDao dao)
    {
        var userId = new UserId(dao.UserId);
        var handle = new UserHandle(dao.Localpart, dao.Domain);

        return new User(userId, handle);
    }
}