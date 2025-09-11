using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class MySqlDatabase(DbServerConnection serverConnection, DbServerTunnel serverTunnel, ILogger logger)
    : DatabaseBase(serverConnection, serverTunnel, logger)
{
    private readonly DbServerConnection _serverConnection = serverConnection;

    protected override async Task PerformBackupInternal(string fullFilePath)
    {
        var host = _serverConnection.IsTunnelRequired ? "127.0.0.1" : _serverConnection.ServerHost;
        var port = _serverConnection.IsTunnelRequired ? serverTunnel.LocalPort : _serverConnection.ServerPort;
        
        var args =
            $"-h {host} " +
            $"-P {port} " +
            $"-u {_serverConnection.DbUser} " +
            $"-p{_serverConnection.DbPasswd} " +
            $"--single-transaction --routines --events " +
            $"{_serverConnection.DbName}";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "mysqldump",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        await using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await process.StandardOutput.BaseStream.CopyToAsync(fileStream);
        }

        var stdErr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"mysqldump exited with code {process.ExitCode} // error: {stdErr}");
    }
}