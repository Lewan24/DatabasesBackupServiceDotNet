using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Interfaces;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class PostgreSqlDatabase(
    DbServerConnection serverConnection,
    ILogger logger)
    : IDatabase
{
    public async Task<string> PerformBackup(ServerBackupsConfiguration serverConfig)
    {
        logger.LogInformation("Performing backup for {DatabaseName}", serverConnection.DbName);

        try
        {
            var fileName = $"{DateTime.Now:yyyy.MM.dd.HH.mm}.sql";
            var backupPath = serverConfig.CreateBackupPath(serverConnection, fileName);
            
            var server = $"{serverConnection.ServerHost}:{serverConnection.ServerPort}";

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
                $"{server}:{serverConnection.DbName}:{serverConnection.DbUser}:{serverConnection.DbPasswd}");

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
                        $@"-h {serverConnection.ServerHost} -p {serverConnection.ServerPort} -U {serverConnection.DbUser} -F c -b -v -f {backupPath.FullFilePath} {serverConnection.DbName}",
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

            return fileName;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, e.Message);
            throw;
        }
    }

    public string GetDatabaseName() 
        => serverConnection.DbName;

    public Guid GetServerId()
        => serverConnection.Id;

    public DbServerConnection GetServerConnection()
        => serverConnection;
}