using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Interfaces;
using Modules.Backup.Core.StaticClasses;
using MySql.Data.MySqlClient;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class MySqlDatabase(
    DbServerConnection serverConnection,
    ILogger logger)
    : IDatabase
{
    public async Task<string> PerformBackup(ServerBackupsConfiguration serverConfig)
    {
        logger.LogInformation("Performing backup for {DatabaseName}", serverConnection.DbName);

        try
        {
            var connectionString = DatabaseBackupStrings.PrepareConnectionString(serverConnection);

            await using var connection = new MySqlConnection(connectionString);
            await using var cmd = new MySqlCommand();
            using var backup = new MySqlBackup(cmd);

            cmd.Connection = connection;
            connection.Open();

            var backupPathFileName = serverConfig.GetBackupPathAndBackupFileName(serverConnection);
            
            backup.ExportToFile(backupPathFileName.Path);
            await connection.CloseAsync();
            
            // todo: move compressing to service
            logger.LogInformation("Performing backup compression...");
            var compressionResult =
                CompressBackupFile.Perform(backupPaths.DatabaseBackupPath, backupPaths.BackupFileName);
            logger.LogInformation("Completed backup for {DatabaseName}. Backup path: {ZipFilePath}", serverConnection.DbName,
                compressionResult);

            return backupPathFileName.FileName;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while performing backup for {DatabaseName}", serverConnection.DbName);
            throw;
        }
    }

    public string GetDatabaseName() 
        => serverConnection.DbName;

    public Guid GetServerId()
        => serverConnection.Id;
}