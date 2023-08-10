using Core.Models;

namespace Application.Data.Interfaces;

public interface IDbBackupService
{
    Task RunService(List<DatabaseConfigModel> dbConfigurations);
    Task<int> GetBackupsCounter();
}