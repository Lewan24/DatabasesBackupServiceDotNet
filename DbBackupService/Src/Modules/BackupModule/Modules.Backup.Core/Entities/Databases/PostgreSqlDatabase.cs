using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.StaticClasses;
using Npgsql;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class PostgreSqlDatabase : DatabaseBase
{
    private readonly DbServerConnection _serverConnection;
    private readonly ILogger _logger;

    public PostgreSqlDatabase(
        DbServerConnection serverConnection, 
        DbServerTunnel serverTunnel, 
        ILogger logger) : base(serverConnection, serverTunnel, logger, null)
    {
        _serverConnection = serverConnection;
        _logger = logger;

        if (!ToolChecker.IsToolAvailable("pg_dump"))
            BackupExtension = ".csv";
    }
    
    protected override async Task PerformBackupInternal(string fullFilePath)
    {
        if (ToolChecker.IsToolAvailable("pg_dump"))
            await PerformBackupWithDump(fullFilePath);
        else
            await PerformBackupWithLibrary(fullFilePath);
    }

    private async Task PerformBackupWithDump(string fullFilePath)
    {
        var hostPort = GetHostAndPort();
        var args = $"--host={hostPort.Host} --port={hostPort.Port} --username={_serverConnection.DbUser} --no-password --dbname={_serverConnection.DbName}";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pg_dump",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment =
                {
                    ["PGPASSWORD"] = _serverConnection.DbPasswd
                }
            }
        };

        await using var fs = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write);
        process.Start();
        await process.StandardOutput.BaseStream.CopyToAsync(fs);
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"pg_dump failed: {error}");
    }

    private async Task PerformBackupWithLibrary(string fullFilePath)
    {
        var hostPort = GetHostAndPort();
        var connStr =
            $"Host={hostPort.Host};Port={hostPort.Port};Database={_serverConnection.DbName};Username={_serverConnection.DbUser };Password={_serverConnection.DbPasswd};";
        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();
        
        _logger.LogInformation("Connection opened.");
        
        var tables = new List<(string Schema, string Table)>();

        await using (var cmd = new NpgsqlCommand(@"
    SELECT schemaname, tablename
    FROM pg_tables
    WHERE schemaname NOT IN ('pg_catalog', 'information_schema', 'pg_toast')
    ORDER BY schemaname, tablename;", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                tables.Add((reader.GetString(0), reader.GetString(1)));
        }

        await using var writer = new StreamWriter(fullFilePath, false, System.Text.Encoding.UTF8);

        foreach (var (schema, table) in tables)
        {
            var fullName = $"{schema}.{table}";
            var copyCmd = $"COPY {fullName} TO STDOUT WITH CSV HEADER";
            using var exporter = await conn.BeginTextExportAsync(copyCmd);

            string? line;
            while ((line = await exporter.ReadLineAsync()) != null)
                await writer.WriteLineAsync(line);

            await writer.WriteLineAsync();
            await writer.FlushAsync();
        }
    }
}
