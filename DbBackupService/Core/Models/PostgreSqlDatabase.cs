using Core.Interfaces;

namespace Core.Models;

public class PostgreSqlDatabase : IDatabase
{
    public Task PerformBackup(DatabaseConfigModel dbConfiguration)
    {
        throw new NotImplementedException();
    }
}