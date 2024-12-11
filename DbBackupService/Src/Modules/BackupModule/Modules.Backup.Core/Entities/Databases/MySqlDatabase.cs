using Modules.Backup.Core.Entities.Models;
using Modules.Backup.Core.Interfaces;
using Modules.Backup.Core.StaticClasses;
using MySql.Data.MySqlClient;
using NLog;

namespace Modules.Backup.Core.Entities.Databases;

public class MySqlDatabase(DatabaseConfigModel databaseConfig, Logger logger, ApplicationConfigurationModel appConfig)
    : IDatabase
{
    private readonly Logger _logger = logger.Factory.GetLogger(nameof(MySqlDatabase));

    public async Task PerformBackup()
    {
        _logger.Info("Performing backup for {DatabaseName}", databaseConfig.DbName);

        try
        {
            var backupPaths = BackupDirectories.CheckDbNameAndPrepareBackupPaths(databaseConfig, appConfig);
            var combinedBackupPathBackupFile =
                await BackupDirectories.PrepareNeededDirectoryAndClean(backupPaths, appConfig, _logger);

            var connectionString = DatabaseBackupStrings.PrepareConnectionString(databaseConfig);

            await using var connection = new MySqlConnection(connectionString);
            await using var cmd = new MySqlCommand();
            using var backup = new MySqlBackup(cmd);

            cmd.Connection = connection;
            connection.Open();
            backup.ExportToFile(combinedBackupPathBackupFile);
            await connection.CloseAsync();

            _logger.Info("Performing backup compression...");
            var compressionResult =
                CompressBackupFile.Perform(backupPaths.DatabaseBackupPath, backupPaths.BackupFileName);
            _logger.Info("Completed backup for {DatabaseName}. Backup path: {ZipFilePath}", databaseConfig.DbName,
                compressionResult);
        }
        catch (Exception e)
        {
            _logger.Debug(e);
            _logger.Warn(e);
            throw;
        }
    }

    public Task<string?> GetDatabaseName()
    {
        return Task.FromResult(databaseConfig.DbName);
    }
}