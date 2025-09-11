using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class MsSqlDatabase(DbServerConnection serverConnection, ILogger logger)
    : DatabaseBase(serverConnection, logger)
{
    private readonly DbServerConnection _serverConnection = serverConnection;

    protected override async Task PerformBackupInternal(string fullFilePath)
    {
        // Używamy sqlcmd + BACKUP DATABASE
        var backupSql = $"BACKUP DATABASE [{_serverConnection.DbName}] TO DISK = N'{fullFilePath}' WITH INIT, FORMAT;";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sqlcmd",
                Arguments =
                    $@"-S {_serverConnection.ServerHost},{_serverConnection.ServerPort} -U {_serverConnection.DbUser} -P {_serverConnection.DbPasswd} -Q ""{backupSql}""",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };

        process.Start();
        var stdErr = await process.StandardError.ReadToEndAsync();
        var stdOut = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"sqlcmd exited with {process.ExitCode} // stderr: {stdErr} // stdout: {stdOut}");
    }
}