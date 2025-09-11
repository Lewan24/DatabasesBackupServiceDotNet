using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Core.Interfaces;

public interface IDatabase
{
    Task<string> PerformBackup(ServerBackupsConfiguration serverConfig);
    string GetDatabaseName();
    Guid GetServerId();
}