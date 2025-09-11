using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Interfaces;

namespace Modules.Backup.Core.Entities.Databases;

public abstract class DatabaseBase(DbServerConnection serverConnection, ILogger logger) : IDatabase
{
    public async Task<string> PerformBackup(ServerBackupsConfiguration serverConfig)
    {
        logger.LogInformation("Performing backup for {DatabaseName}", serverConnection.DbName);

        var fileName = $"{DateTime.Now:yyyy.MM.dd.HH.mm}.sql";
        var backupPath = serverConfig.CreateBackupPath(serverConnection, fileName);

        try
        {
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