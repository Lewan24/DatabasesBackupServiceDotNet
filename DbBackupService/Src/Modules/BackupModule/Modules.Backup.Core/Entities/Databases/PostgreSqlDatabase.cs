using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class PostgreSqlDatabase(DbServerConnection serverConnection, ILogger logger)
    : DatabaseBase(serverConnection, logger)
{
    private readonly DbServerConnection _serverConnection = serverConnection;

    protected override async Task PerformBackupInternal(string fullFilePath)
    {
        var server = $"{_serverConnection.ServerHost}:{_serverConnection.ServerPort}";
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

        await RunProcess("pg_dump",
            $@"-h {_serverConnection.ServerHost} -p {_serverConnection.ServerPort} -U {_serverConnection.DbUser} -F c -b -v -f {fullFilePath} {_serverConnection.DbName}");

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

        process.Start();
        var stdErr = await process.StandardError.ReadToEndAsync();
        var stdOut = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"{fileName} exited with {process.ExitCode} // stderr: {stdErr} // stdout: {stdOut}");
    }
}
