using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Core.Entities.Models;
using Core.Interfaces;
using Core.StaticClassess;
using NLog;

namespace Core.Entities.Databases;

public class PostgreSqlDatabase : IDatabase
{
    private readonly DatabaseConfigModel _databaseConfig;
    private readonly ApplicationConfigurationModel _appConfig;
    private readonly Logger _logger;

    public PostgreSqlDatabase(DatabaseConfigModel databaseConfig, Logger logger, ApplicationConfigurationModel appConfig)
    {
        _databaseConfig = databaseConfig;
        _appConfig = appConfig;
        _logger = logger.Factory.GetLogger(nameof(PostgreSqlDatabase));
    }

    public async Task<bool> PerformBackup()
    {
        _logger.Info("Performing backup for {DatabaseName}", _databaseConfig.DbName);

        try
        {
            if (string.IsNullOrWhiteSpace(_databaseConfig.DbName))
                throw new ArgumentNullException(nameof(_databaseConfig.DbName));

            var backupPaths = PrepareDatabaseBackupStrings.PrepareBackupPaths(_databaseConfig, _appConfig);

            if (!Directory.Exists(backupPaths.DatabaseBackupPath))
                Directory.CreateDirectory(backupPaths.DatabaseBackupPath);

            var combinedBackupPathBackupFile = Path.Combine(backupPaths.DatabaseBackupPath, backupPaths.BackupFileName);

            if (File.Exists(combinedBackupPathBackupFile))
                File.Delete(combinedBackupPathBackupFile);

            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = "pg_dump",
                Arguments = $@"-d postgres://{_databaseConfig.DbUser}:{_databaseConfig.DbPasswd}@{_databaseConfig.DbServerAndPort}/{_databaseConfig.DbName} -f ""{combinedBackupPathBackupFile}"" -F c",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
            process.Close();

            var compressionResult = CompressBackupFile.Perform(backupPaths.DatabaseBackupPath, backupPaths.BackupFileName);
            _logger.Info("Completed backup for {DatabaseName}. Backup path: {ZipFilePath}", _databaseConfig.DbName, compressionResult);

            return true;
        }
        catch (Exception e)
        {
            _logger.Debug(e);
            _logger.Warn(e);

            return false;
        }
    }
}
