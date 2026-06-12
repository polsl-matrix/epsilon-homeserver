using System.Data;

namespace Tesseract.Infrastructure.Common.Database.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}