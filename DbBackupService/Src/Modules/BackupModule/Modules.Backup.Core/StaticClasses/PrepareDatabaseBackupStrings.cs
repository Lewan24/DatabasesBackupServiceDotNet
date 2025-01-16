using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Entities.Models;

namespace Modules.Backup.Core.StaticClasses;

public static class DatabaseBackupStrings
{
    public static string PrepareConnectionString(DbConnection configuration)
    {
        var server = configuration.DbServerPort?.Split(':');
        var connectionString =
            $"Server={server![0]};Port={server[1]};Database={configuration.DbName};Uid={configuration.DbUser};Pwd={configuration.DbPasswd};";

        return connectionString;
    }

    public static (string DatabaseBackupPath, string BackupFileName) PrepareBackupPaths(
        DbConnection databaseConfiguration, ApplicationConfigurationModel appConfig)
    {
        var fileName = $"{databaseConfiguration.DbName}.sql";

        var server = databaseConfiguration.DbServerPort!.Split(':');
        var backupPath = Path.Combine(appConfig.BackupSaveDirectory!,
            $"{databaseConfiguration.DbName!}_{server[0]}_{server[1]}");

        return new ValueTuple<string, string>(backupPath, fileName);
    }
}