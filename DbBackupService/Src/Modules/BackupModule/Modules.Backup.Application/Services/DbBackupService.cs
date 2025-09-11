using Microsoft.Extensions.Logging;
using Modules.Backup.Application.Interfaces;
using Modules.Backup.Core.Entities.Databases;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Interfaces;
using Modules.Backup.Core.StaticClasses;
using Modules.Backup.Infrastructure.DbContexts;
using Modules.Backup.Shared.Enums;
using OneOf;
using OneOf.Types;

namespace Modules.Backup.Application.Services;

internal sealed class DbBackupService(
    BackupsDbContext dbContext,
    ILogger<DbBackupService> logger)
    : IDbBackupService
{
    public async Task<OneOf<Success, string>> BackupFromSchedules()
    {
        logger.LogInformation("Checking for any pending schedules...");

        var pendingSchedulesServersList = dbContext.Schedules
            .Where(x => x.IsEnabled && DateTime.Now >= x.NextBackupDate)
            .Select(x => x.DbConnectionId)
            .ToList();
        if (!pendingSchedulesServersList.Any())
        {
            logger.LogInformation("No pending schedules found.");
            return new Success();
        }
        
        var servers = dbContext.DbConnections
            .Where(x => pendingSchedulesServersList.Contains(x.Id))
            .ToList();
        if (!servers.Any())
        {
            logger.LogInformation("No servers assigned to schedules found.");
            return "No servers assigned to schedules found.";
        }
        
        logger.LogInformation("Preparing backups for {ServersCount} databases...", servers.Count);

        await PerformBackup(servers);

        logger.LogInformation("{ServiceName} finished its job.", nameof(DbBackupService));
        
        return new Success();
    }

    public async Task<OneOf<Success, string>> BackupDb(Guid serverId)
    {
        logger.LogInformation("Preparing backup for server: [{ServerId}]", serverId);
        
        var server = dbContext.DbConnections.FirstOrDefault(x => x.Id == serverId);
        if (server is null)
        {
            logger.LogInformation("No server found with id {ServerId}.", serverId);
            return "Can't find server";
        }
        
        await PerformBackup([server]);

        logger.LogInformation("{ServiceName} finished its job.", nameof(DbBackupService));
        
        return new Success();
    }
    
    private async Task PerformBackup(List<DbServerConnection> serversConnections)
    {
        try
        {
            var databasesList = new List<IDatabase>();

            foreach (var serverConn in serversConnections)
            {
                IDatabase database = serverConn.DbType switch
                {
                    DatabaseType.MySql => new MySqlDatabase(serverConn, logger),
                    DatabaseType.PostgreSql => new PostgreSqlDatabase(serverConn, logger),
                    _ => throw new NotSupportedException(
                        $"Selected type of database is not supported: {serverConn.DbType}")
                };

                databasesList.Add(database);
            }

            foreach (var db in databasesList)
                try
                {
                    // TODO: implement removing db entries on successful clearing old backups
                    // TODO: implement adding backup entry to db after successful backup
                    var serverConfig = dbContext.Configurations.FirstOrDefault(x => x.ServerId == db.GetServerId());
                    if (serverConfig is null)
                    {
                        logger.LogWarning("Can't find any backups configuration for server {ServerId}", db.GetServerId());
                        continue;
                    }
                    
                    var backupPath = serverConfig.CreateBackupPath(db.GetServerConnection(), null);
                    await DeleteOldBackup.Delete(backupPath.DirectoryPath, serverConfig.TimeInDaysToHoldBackups);
                    
                    var createdFileName = await db.PerformBackup(serverConfig);
                    var compressedFilename = CompressBackupFile.Perform(backupPath.DirectoryPath, createdFileName);
                    logger.LogInformation("Successfully created and compressed backup: [{Database}], [{ZipFile}]", db.GetDatabaseName(), compressedFilename);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Exception thrown while performing backup in database");
                }
        }
        catch (NotSupportedException e)
        {
            logger.LogWarning(e, "Exception thrown while preparing databases");
        }
    }
}