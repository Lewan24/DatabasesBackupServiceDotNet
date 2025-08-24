using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Application.Interfaces;

public interface IDbBackupService
{
    Task RunService(List<DbServerConnection> dbConfigurations);
    Task<int> GetBackupsCounter();
}