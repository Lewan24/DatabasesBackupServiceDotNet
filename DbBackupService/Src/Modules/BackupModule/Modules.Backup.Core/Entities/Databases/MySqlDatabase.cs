using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.StaticClasses;
using Modules.Crypto.Shared.Interfaces;
using MySql.Data.MySqlClient;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class MySqlDatabase(DbServerConnection serverConnection, DbServerTunnel serverTunnel, ILogger logger, ICryptoService cryptoService)
    : DatabaseBase(serverConnection, serverTunnel, logger, null, cryptoService)
{
    private readonly DbServerConnection _serverConnection = serverConnection;
    private readonly ICryptoService _cryptoService = cryptoService;

    protected override async Task PerformBackupInternal(string fullFilePath)
    {
        if (ToolChecker.IsToolAvailable("mysqldump"))
            await PerformBackupWithDump(fullFilePath);
        else
            await PerformBackupWithLibrary(fullFilePath);
    }

    private async Task PerformBackupWithDump(string fullFilePath)
    {
        var hostPort = GetHostAndPort();

        var decryptedDbPassword = _cryptoService.Decrypt(_serverConnection.DbPasswd);
        var args = $"-h {hostPort.Host} -P {hostPort.Port} -u {_serverConnection.DbUser} -p{decryptedDbPassword} {_serverConnection.DbName}";

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

        await using var fs = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write);
        process.Start();
        await process.StandardOutput.BaseStream.CopyToAsync(fs);
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"mysqldump failed: {error}");
    }

    private async Task PerformBackupWithLibrary(string fullFilePath)
    {
        var hostPort = GetHostAndPort();
        
        var decryptedDbPassword = _cryptoService.Decrypt(_serverConnection.DbPasswd);
        var connStr = $"Server={hostPort.Host};Port={hostPort.Port};Database={_serverConnection.DbName};Uid={_serverConnection.DbUser};Pwd={decryptedDbPassword};";

        await using var conn = new MySqlConnection(connStr);
        await using var cmd = new MySqlCommand();
        using var backup = new MySqlBackup(cmd);

        cmd.Connection = conn;
        await conn.OpenAsync();

        backup.ExportToFile(fullFilePath);

        await conn.CloseAsync();
    }
}
