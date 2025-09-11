using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Interfaces;

namespace Modules.Backup.Core.Entities.Databases;

public abstract class DatabaseBase : IDatabase
{
    protected readonly DbServerConnection ServerConnection;
    protected readonly ILogger Logger;

    protected DatabaseBase(DbServerConnection serverConnection, ILogger logger)
    {
        ServerConnection = serverConnection;
        Logger = logger;
    }

    public async Task<string> PerformBackup(ServerBackupsConfiguration serverConfig)
    {
        Logger.LogInformation("Performing backup for {DatabaseName}", ServerConnection.DbName);

        var fileName = $"{DateTime.Now:yyyy.MM.dd.HH.mm}.sql";
        var backupPath = serverConfig.CreateBackupPath(ServerConnection, fileName);

        try
        {
            await PerformBackupInternal(backupPath.FullFilePath!);
            return fileName;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while performing backup for {DatabaseName}", ServerConnection.DbName);

            try
            {
                if (File.Exists(backupPath.FullFilePath))
                    File.Delete(backupPath.FullFilePath);
            }
            catch (Exception cleanupEx)
            {
                Logger.LogWarning(cleanupEx, "Failed to clean up corrupted backup file {File}", backupPath.FullFilePath);
            }

            throw;
        }
    }

    protected abstract Task PerformBackupInternal(string fullFilePath);

    public string GetDatabaseName() => ServerConnection.DbName;
    public Guid GetServerId() => ServerConnection.Id;
    public DbServerConnection GetServerConnection() => ServerConnection;
}