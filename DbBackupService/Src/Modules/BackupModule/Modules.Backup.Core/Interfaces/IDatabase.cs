namespace Modules.Backup.Core.Interfaces;

public interface IDatabase
{
    Task PerformBackup();
    Task<string?> GetDatabaseName();
}