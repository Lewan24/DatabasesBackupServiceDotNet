using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Interfaces;
using Modules.Backup.Core.StaticClasses;
using Modules.Backup.Shared.Enums;

namespace Modules.Backup.Core.Entities.Databases;

public abstract class DatabaseBase(DbServerConnection serverConnection, DbServerTunnel serverTunnel, ILogger logger, string? defaulBackupExtension) : IDatabase
{
    protected string BackupExtension = defaulBackupExtension ?? ".sql";
    
    public async Task<string> PerformBackup(ServerBackupsConfiguration serverConfig)
    {
        logger.LogInformation("Performing backup for {DatabaseName}", serverConnection.DbName);

        string fileName = $"{DateTime.Now:yyyy.MM.dd.HH.mm}{BackupExtension}";
        logger.LogInformation("Potential backup file name: {FileName}", fileName);
        
        var backupPath = serverConfig.CreateBackupPath(serverConnection, fileName);
        logger.LogInformation("Prepared backup path: {BackupPath}", backupPath.DirectoryPath);
        
        try
        {
            using var tunnel = serverConnection.IsTunnelRequired
                ? SshTunnelHelper.OpenTunnel(serverConnection, serverTunnel)
                : null;
            
            logger.LogInformation("Performing internal backup operation for file: {FullFilePath}", backupPath.FullFilePath);
            await PerformBackupInternal(backupPath.FullFilePath!);
            return fileName;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while performing backup for {DatabaseName}", serverConnection.DbName);

            try
            {
                if (File.Exists(backupPath.FullFilePath))
                    File.Delete(backupPath.FullFilePath);
            }
            catch (Exception cleanupEx)
            {
                logger.LogWarning(cleanupEx, "Failed to clean up corrupted backup file {File}", backupPath.FullFilePath);
            }

            throw;
        }
    }

    protected abstract Task PerformBackupInternal(string fullFilePath);
    protected (string Host, int Port) GetHostAndPort()
    {
        var host = serverConnection.IsTunnelRequired ? "127.0.0.1" : serverConnection.ServerHost;
        var port = serverConnection.IsTunnelRequired ? serverTunnel.LocalPort : serverConnection.ServerPort;

        return (host, port);
    }
    public string GetDatabaseName() => serverConnection.DbName;
    public Guid GetServerId() => serverConnection.Id;
    public DbServerConnection GetServerConnection() => serverConnection;

    public string GetBackupExtension()
        => BackupExtension;
}