using Tesseract.Application.ClientServer.Common.Repositories;
using Tesseract.Domain.Users;
using Tesseract.Infrastructure.ClientServer.Common.Dao;

namespace Tesseract.Infrastructure.ClientServer.Common.Mappers;

internal static class ResponseMapper
{
    public static Response ToDomain(this ResponseDao dao, string path)
    {
        return new Response
        {
            UserId = new UserId(dao.UserId),
            Path = path,
            StatusCode = dao.StatusCode,
            ContentType = dao.ContentType,
            Content = dao.Content,
        };
    }
}