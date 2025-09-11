using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class PostgreSqlDatabase(DbServerConnection serverConnection, DbServerTunnel serverTunnel, ILogger logger)
    : DatabaseBase(serverConnection, serverTunnel, logger)
{
    private readonly DbServerConnection _serverConnection = serverConnection;

    protected override async Task PerformBackupInternal(string fullFilePath)
    {
        var host = _serverConnection.IsTunnelRequired ? "127.0.0.1" : _serverConnection.ServerHost;
        var port = _serverConnection.IsTunnelRequired ? serverTunnel.LocalPort : _serverConnection.ServerPort;
        
        var server = $"{host}:{port}";
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
            $"{server}:{_serverConnection.DbName}:{_serverConnection.DbUser}:{_serverConnection.DbPasswd}");

        if (isLinux)
        {
            await RunProcess("chmod", $"0600 {pgPassFilePath}");
        }

        var arguments = $"-h {host} -p {port} " +
                        $"-U {_serverConnection.DbUser} -F c -b -v -f \"{fullFilePath}\" {_serverConnection.DbName}";
        
        await RunProcess("pg_dump", arguments);

        await File.WriteAllTextAsync(pgPassFilePath, "Cleaned");
    }

    private static async Task RunProcess(string fileName, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
        
        process.StartInfo.EnvironmentVariables["PGSSLMODE"] = "prefer";
        
        process.Start();
        var stdErr = await process.StandardError.ReadToEndAsync();
        var stdOut = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"{fileName} exited with {process.ExitCode} // stderr: {stdErr} // stdout: {stdOut}");
    }
}
