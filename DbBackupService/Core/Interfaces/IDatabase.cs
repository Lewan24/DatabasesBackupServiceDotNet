using Core.Models;

namespace Core.Interfaces;

public interface IDatabase
{
    Task PerformBackup(DatabaseConfigModel dbConfiguration);
}