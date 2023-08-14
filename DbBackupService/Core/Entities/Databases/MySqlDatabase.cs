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
            if (string.IsNullOrWhiteSpace(_databaseConfig.DbName))
                throw new ArgumentNullException(nameof(_databaseConfig.DbName));

            var connectionString = PrepareDatabaseBackupStrings.PrepareConnectionString(_databaseConfig);
            var backupPaths = PrepareDatabaseBackupStrings.PrepareBackupPaths(_databaseConfig, _appConfig);

            if (!Directory.Exists(backupPaths.DatabaseBackupPath))
                Directory.CreateDirectory(backupPaths.DatabaseBackupPath);

            var combinedBackupPathBackupFile = Path.Combine(backupPaths.DatabaseBackupPath, backupPaths.BackupFileName);

            if (File.Exists(combinedBackupPathBackupFile))
                File.Delete(combinedBackupPathBackupFile);

            await using var connection = new MySqlConnection(connectionString);
            await using var cmd = new MySqlCommand();
            using var backup = new MySqlBackup(cmd);

            cmd.Connection = connection;
            connection.Open();
            backup.ExportToFile(combinedBackupPathBackupFile);
            await connection.CloseAsync();

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