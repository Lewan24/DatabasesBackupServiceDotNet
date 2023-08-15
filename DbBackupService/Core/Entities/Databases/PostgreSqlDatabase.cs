using System.Diagnostics;
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

    public async Task PerformBackup()
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

			var server = _databaseConfig.DbServerAndPort!.Split(':');
            var startInfo = new ProcessStartInfo
            {
                FileName = "pg_dump",
				Arguments = $@"-Fc ""host={server[0]} port={server[1]} dbname={_databaseConfig.DbName} user={_databaseConfig.DbUser} password={_databaseConfig.DbPasswd}"" > {combinedBackupPathBackupFile}",
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            var process = new Process()
            {
                StartInfo = startInfo
            };

            process.Start();
            
            var processError = await process.StandardError.ReadToEndAsync();

            // TODO: For some reason, the pg_dump is not returning exit status to process, so the .net is just running other code like checking and compressing (actually downloading file) so throws errors
            // Problem is the big file, anyway, it needs to wait enough time to download it, and wait for exit or return success.
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"pg_dump exited with code: {process.ExitCode} instead of 0 // error: {processError}");

            process.Close();

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
