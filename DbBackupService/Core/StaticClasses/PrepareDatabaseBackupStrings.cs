using Core.Entities.Models;

namespace Core.StaticClasses;

public static class DatabaseBackupStrings
{
    public static string PrepareConnectionString(DatabaseConfigModel configuration)
    {
        var server = configuration.DbServerAndPort?.Split(':');
        var connectionString =
            $"Server={server![0]};Port={server[1]};Database={configuration.DbName};Uid={configuration.DbUser};Pwd={configuration.DbPasswd};";

        return connectionString;
    }

    public static (string DatabaseBackupPath, string BackupFileName) PrepareBackupPaths(
        DatabaseConfigModel databaseConfiguration, ApplicationConfigurationModel appConfig)
    {
        var fileName = $"{databaseConfiguration.DbName}.sql";

        var server = databaseConfiguration.DbServerAndPort!.Split(':');
        var backupPath = Path.Combine(appConfig.BackupSaveDirectory!,
            $"{databaseConfiguration.DbName!}_{server[0]}_{server[1]}");

        return new ValueTuple<string, string>(backupPath, fileName);
    }
}