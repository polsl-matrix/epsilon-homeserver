using System.Data.Common;

namespace Tesseract.Infrastructure.Common.Database.Interfaces;

internal interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}