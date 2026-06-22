using Tesseract.Domain.Users;
using Tesseract.Infrastructure.ClientServer.Identity.Dao;

namespace Tesseract.Infrastructure.ClientServer.Identity.Mappers;

internal static class ProfileMapper
{
    public static Profile ToDomain(this ProfileDao dao)
    {
        var userId = new UserId(dao.UserId);

        return new Profile(userId, dao.DisplayName, dao.AvatarUrl);
    }
}