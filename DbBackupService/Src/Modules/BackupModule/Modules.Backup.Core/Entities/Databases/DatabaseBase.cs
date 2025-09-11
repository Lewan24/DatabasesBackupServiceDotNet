using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Interfaces;
using Modules.Backup.Core.StaticClasses;
using Modules.Backup.Shared.Enums;

namespace Modules.Backup.Core.Entities.Databases;

public abstract class DatabaseBase(DbServerConnection serverConnection, DbServerTunnel serverTunnel, ILogger logger) : IDatabase
{
    public async Task<string> PerformBackup(ServerBackupsConfiguration serverConfig)
    {
        logger.LogInformation("Performing backup for {DatabaseName}", serverConnection.DbName);

        string fileName = $"{DateTime.Now:yyyy.MM.dd.HH.mm}";

        if (serverConnection.DbType is DatabaseType.SqlServer)
            fileName += ".bak";
        else
            fileName += ".sql";
        
        var backupPath = serverConfig.CreateBackupPath(serverConnection, fileName);

        try
        {
            using var tunnel = serverConnection.IsTunnelRequired
                ? SshTunnelHelper.OpenTunnel(serverConnection, serverTunnel)
                : null;
            
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

    public string GetDatabaseName() => serverConnection.DbName;
    public Guid GetServerId() => serverConnection.Id;
    public DbServerConnection GetServerConnection() => serverConnection;
}