using Core.Entities.Models;
using NLog;

namespace Core.StaticClassess;

public static class PrepareBackupDirectories
{
    public static (string DatabaseBackupPath, string BackupFileName) CheckDbNameAndPrepareBackupPaths(DatabaseConfigModel databaseConfig, ApplicationConfigurationModel appConfig)
    {
        if (string.IsNullOrWhiteSpace(databaseConfig.DbName))
            throw new ArgumentNullException(nameof(databaseConfig.DbName));

        var backupPaths = PrepareDatabaseBackupStrings.PrepareBackupPaths(databaseConfig, appConfig);
        return backupPaths;
    }

    public static async Task<string> PrepareNeededDirectoryAndClean((string DatabaseBackupPath, string BackupFileName) backupPaths, ApplicationConfigurationModel appConfig, Logger logger)
    {
        if (!Directory.Exists(backupPaths.DatabaseBackupPath))
            Directory.CreateDirectory(backupPaths.DatabaseBackupPath);

        logger.Info("Deleting old backups...");
        await DeleteOldBackup.Delete(backupPaths.DatabaseBackupPath, appConfig.TimeInDaysToHoldBackups, logger);
            
        var combinedBackupPathBackupFile = Path.Combine(backupPaths.DatabaseBackupPath, backupPaths.BackupFileName);

        if (File.Exists(combinedBackupPathBackupFile))
            File.Delete(combinedBackupPathBackupFile);

        return combinedBackupPathBackupFile;
    }
}