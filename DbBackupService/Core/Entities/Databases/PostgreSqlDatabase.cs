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
            
            var userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var pgpassFilePath = "";
            
            var isLinux = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
            if (isLinux)
                pgpassFilePath = Path.Combine(userFolderPath, ".pgpass");
            else
            {
                var postgresConfDirectory = Path.Combine(userFolderPath, "AppData", "Roaming", "postgresql");
                if (!Directory.Exists(postgresConfDirectory))
                    Directory.CreateDirectory(postgresConfDirectory);

                pgpassFilePath = Path.Combine(postgresConfDirectory, "pgpass.conf");
            }
            
            await File.WriteAllTextAsync(pgpassFilePath, $"{server[0]}:{server[1]}:{_databaseConfig.DbName}:{_databaseConfig.DbUser}:{_databaseConfig.DbPasswd}");
            
            if (isLinux)
            {
                var processChmod = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"0600 {pgpassFilePath}",
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                processChmod.Start();
                var processChmodErrors = await processChmod.StandardError.ReadToEndAsync();
                await processChmod.WaitForExitAsync();

                if (processChmod.ExitCode != 0)
                    throw new Exception($".pgpass chmod modification process exited with code: {processChmod.ExitCode} instead of 0 // error: {processChmodErrors}");

                processChmod.Close();
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pg_dump",
                    Arguments = $@"-h {server[0]} -p {server[1]} -U {_databaseConfig.DbUser} -F c -b -v -f {combinedBackupPathBackupFile} {_databaseConfig.DbName}",
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            var processError = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"pg_dump exited with code: {process.ExitCode} instead of 0 // error: {processError}");

            process.Close();

            await File.WriteAllTextAsync(pgpassFilePath, "Cleaned");
            
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
