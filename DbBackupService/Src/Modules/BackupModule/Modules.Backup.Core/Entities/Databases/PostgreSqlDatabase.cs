using System.Diagnostics;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Entities.Models;
using Modules.Backup.Core.Interfaces;
using Modules.Backup.Core.StaticClasses;
using NLog;

namespace Modules.Backup.Core.Entities.Databases;

public class PostgreSqlDatabase(
    DbConnection databaseConfig,
    Logger logger,
    ApplicationConfigurationModel appConfig)
    : IDatabase
{
    private readonly Logger _logger = logger.Factory.GetLogger(nameof(PostgreSqlDatabase));

    public async Task PerformBackup()
    {
        _logger.Info("Performing backup for {DatabaseName}", databaseConfig.DbName);

        try
        {
            var backupPaths = BackupDirectories.CheckDbNameAndPrepareBackupPaths(databaseConfig, appConfig);
            var combinedBackupPathBackupFile =
                await BackupDirectories.PrepareNeededDirectoryAndClean(backupPaths, appConfig, _logger);

            var server = databaseConfig.DbServerPort!.Split(':');

            var userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pgPassFilePath;

            var isLinux = Environment.OSVersion.Platform == PlatformID.Unix ||
                          Environment.OSVersion.Platform == PlatformID.MacOSX;
            if (isLinux)
            {
                pgPassFilePath = Path.Combine(userFolderPath, ".pgpass");
            }
            else
            {
                var postgresConfDirectory = Path.Combine(userFolderPath, "AppData", "Roaming", "postgresql");
                if (!Directory.Exists(postgresConfDirectory))
                    Directory.CreateDirectory(postgresConfDirectory);

                pgPassFilePath = Path.Combine(postgresConfDirectory, "pgpass.conf");
            }

            await File.WriteAllTextAsync(pgPassFilePath,
                $"{server[0]}:{server[1]}:{databaseConfig.DbName}:{databaseConfig.DbUser}:{databaseConfig.DbPasswd}");

            if (isLinux)
            {
                var processChmod = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"0600 {pgPassFilePath}",
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                processChmod.Start();
                var processChmodErrors = await processChmod.StandardError.ReadToEndAsync();
                await processChmod.WaitForExitAsync();

                if (processChmod.ExitCode != 0)
                    throw new Exception(
                        $".pgpass chmod modification process exited with code: {processChmod.ExitCode} instead of 0 // error: {processChmodErrors}");

                processChmod.Close();
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pg_dump",
                    Arguments =
                        $@"-h {server[0]} -p {server[1]} -U {databaseConfig.DbUser} -F c -b -v -f {combinedBackupPathBackupFile} {databaseConfig.DbName}",
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            var processError = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception(
                    $"pg_dump exited with code: {process.ExitCode} instead of 0 // error: {processError}");

            process.Close();

            await File.WriteAllTextAsync(pgPassFilePath, "Cleaned");

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