using Core.Interfaces;
using NLog;
using MySql.Data.MySqlClient;
using Core.Entities.Models;
using Core.StaticClassess;

namespace Core.Entities.Databases;

public class MySqlDatabase : IDatabase
{
    private readonly DatabaseConfigModel _databaseConfig;
    private readonly ApplicationConfigurationModel _appConfig;
    private readonly Logger _logger;

    public MySqlDatabase(DatabaseConfigModel databaseConfig, Logger logger, ApplicationConfigurationModel appConfig)
    {
        _databaseConfig = databaseConfig;
        _appConfig = appConfig;
        _logger = logger.Factory.GetLogger(nameof(MySqlDatabase));
    }

    public async Task PerformBackup()
    {
        _logger.Info("Performing backup for {DatabaseName}", _databaseConfig.DbName);

        try
        {
            var backupPaths = PrepareBackupDirectories.CheckDbNameAndPrepareBackupPaths(_databaseConfig, _appConfig);
            var combinedBackupPathBackupFile = await PrepareBackupDirectories.PrepareNeededDirectoryAndClean(backupPaths, _appConfig, _logger);

            var connectionString = PrepareDatabaseBackupStrings.PrepareConnectionString(_databaseConfig);
            
            await using var connection = new MySqlConnection(connectionString);
            await using var cmd = new MySqlCommand();
            using var backup = new MySqlBackup(cmd);

            cmd.Connection = connection;
            connection.Open();
            backup.ExportToFile(combinedBackupPathBackupFile);
            await connection.CloseAsync();

            _logger.Info("Performing backup compression...");
            var compressionResult = CompressBackupFile.Perform(backupPaths.DatabaseBackupPath, backupPaths.BackupFileName);
            _logger.Info("Completed backup for {DatabaseName}. Backup path: {ZipFilePath}", _databaseConfig.DbName, compressionResult);
        }
        catch (Exception e)
        {
            _logger.Debug(e);
            _logger.Warn(e);
            throw;
        }
    }

    public Task<string?> GetDatabaseName() => Task.FromResult(_databaseConfig.DbName);
}