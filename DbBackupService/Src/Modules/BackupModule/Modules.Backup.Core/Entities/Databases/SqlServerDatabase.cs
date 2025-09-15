using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.StaticClasses;
using Modules.Crypto.Shared.Interfaces;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class SqlServerDatabase(
    DbServerConnection serverConnection,
    DbServerTunnel serverTunnel,
    ILogger logger,
    ICryptoService cryptoService)
    : DatabaseBase(serverConnection, serverTunnel, logger, ".bak")
{
    private readonly DbServerConnection _serverConnection = serverConnection;

    protected override async Task PerformBackupInternal(string fullFilePath)
    {
        if (ToolChecker.IsToolAvailable("sqlcmd"))
            await PerformBackupWithSqlCmd(fullFilePath);
        else
            await PerformBackupWithSmo(fullFilePath);
    }

    private async Task PerformBackupWithSqlCmd(string fullFilePath)
    {
        var hostPort = GetHostAndPort();
        var decryptedDbPasswd = cryptoService.Decrypt(_serverConnection.DbPasswd);
        var args = $"-S {hostPort.Host},{hostPort.Port} -U {_serverConnection.DbUser} -P {decryptedDbPasswd} " +
                   $"-Q \"BACKUP DATABASE [{_serverConnection.DbName}] TO DISK='{fullFilePath}' WITH INIT\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sqlcmd",
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"sqlcmd failed: {error}");
    }

    private async Task PerformBackupWithSmo(string fullFilePath)
    {
        var hostPort = GetHostAndPort();
        var decryptedDbPasswd = cryptoService.Decrypt(_serverConnection.DbPasswd);
        var connStr = $"Data Source={hostPort.Host},{hostPort.Port};" +
                      $"Initial Catalog={_serverConnection.DbName};" +
                      $"User ID={_serverConnection.DbUser};" +
                      $"Password={decryptedDbPasswd};" +
                      $"Encrypt=True;TrustServerCertificate=True;";

        var sqlConn = new SqlConnection(connStr);
        var serverConn = new ServerConnection(sqlConn);
        var server = new Server(serverConn);

        var backup = new Microsoft.SqlServer.Management.Smo.Backup
        {
            Action = BackupActionType.Database,
            Database = _serverConnection.DbName
        };

        backup.Devices.AddDevice(fullFilePath, DeviceType.File);
        backup.Initialize = true;

        await Task.Run(() => backup.SqlBackup(server));
    }
}