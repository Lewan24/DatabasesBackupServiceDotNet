using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Entities.Models;

namespace Modules.Backup.Core.StaticClasses;

public static class DatabaseBackupStrings
{
    public static string PrepareConnectionString(DbServerConnection configuration)
    {
        var connectionString =
            $"Server={configuration.ServerHost};Port={configuration.ServerPort};Database={configuration.DbName};Uid={configuration.DbUser};Pwd={configuration.DbPasswd};";

        return connectionString;
    }

    public static (string DatabaseBackupPath, string BackupFileName) PrepareBackupPaths(
        DbServerConnection databaseConfiguration, ApplicationConfigurationModel appConfig)
    {
        var fileName = $"{databaseConfiguration.DbName}.sql";

        var backupPath = Path.Combine(appConfig.BackupSaveDirectory!,
            $"{databaseConfiguration.DbName!}_{databaseConfiguration.ServerHost}_{databaseConfiguration.ServerPort}");

        return new ValueTuple<string, string>(backupPath, fileName);
    }
}