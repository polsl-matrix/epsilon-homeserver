using System.Data.Common;

namespace Tesseract.Infrastructure.Common.Database.Interfaces;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}