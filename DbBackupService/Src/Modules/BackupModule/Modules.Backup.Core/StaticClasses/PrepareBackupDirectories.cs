using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Entities.Models;
using NLog;

namespace Modules.Backup.Core.StaticClasses;

public static class BackupDirectories
{
    public static (string DatabaseBackupPath, string BackupFileName) CheckDbNameAndPrepareBackupPaths(
        DbConnection databaseConfig, ApplicationConfigurationModel appConfig)
    {
        if (string.IsNullOrWhiteSpace(databaseConfig.DbName))
            throw new ArgumentNullException(nameof(databaseConfig.DbName));

        var backupPaths = DatabaseBackupStrings.PrepareBackupPaths(databaseConfig, appConfig);
        return backupPaths;
    }

    public static async Task<string> PrepareNeededDirectoryAndClean(
        (string DatabaseBackupPath, string BackupFileName) backupPaths, ApplicationConfigurationModel appConfig,
        Logger logger)
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