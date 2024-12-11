using Modules.Backup.Core.Entities.Models;

namespace Modules.Backup.Application.Interfaces;

public interface IDbBackupService
{
    Task RunService(List<DatabaseConfigModel> dbConfigurations);
    Task<int> GetBackupsCounter();
}