namespace Core.Interfaces;

public interface IDatabase
{
    Task PerformBackup();
    Task<string?> GetDatabaseName();
}