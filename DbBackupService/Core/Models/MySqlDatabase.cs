using Core.Interfaces;

namespace Core.Models;

public class MySqlDatabase : IDatabase
{
    public Task PerformBackup(DatabaseConfigModel dbConfiguration)
    {
        throw new NotImplementedException();
    }
}