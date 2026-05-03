using System.Data;

namespace Tesseract.Infrastructure.Common.Database;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}