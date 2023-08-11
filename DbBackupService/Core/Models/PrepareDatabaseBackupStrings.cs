using System.IO;
using Core.Entities;

namespace Core.Models;

public static class PrepareDatabaseBackupStrings
{
    public static string PrepareConnectionString(DatabaseConfigModel configuration)
    {
        var server = configuration.DbServerAndPort?.Split(':');
        var connectionString = $"Server={server![0]};Port={server![1]};Database={configuration.DbName};Uid={configuration.DbUser};Pwd={configuration.DbPasswd};";

        return connectionString;
    }

    public static (string DatabaseBackupPath, string BackupFileName) PrepareBackupPaths(DatabaseConfigModel databaseConfiguration, ApplicationConfiguration appConfig)
    {
        var fileName = $"{databaseConfiguration.DbName}.sql";
        var backupPath = Path.Combine(appConfig.BackupSaveDirectory!,
            $"{databaseConfiguration.DbName!}_{databaseConfiguration.DbServerAndPort!}");
        
        return new(backupPath, fileName);
    }
}