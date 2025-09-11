using Microsoft.Extensions.Logging;
using Modules.Backup.Application.Interfaces;
using Modules.Backup.Core.Entities.Databases;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Interfaces;
using Modules.Backup.Infrastructure.DbContexts;
using Modules.Backup.Shared.Enums;

namespace Modules.Backup.Application.Services;

internal sealed class DbBackupService(
    BackupsDbContext dbContext,
    ILogger<DbBackupService> logger)
    : IDbBackupService
{
    public async Task RunService(List<DbServerConnection> dbConfigurations)
    {
        logger.LogInformation("Preparing backups for {DbCConfigsCount} databases...", dbConfigurations.Count);

        await PerformBackup(dbConfigurations);

        logger.LogInformation("{ServiceName} finished its job.", nameof(DbBackupService));
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
                    // TODO: implement cleaning old backups here
                    // TODO: implement removing db entries on successful clearing old backups
                    // TODO: move compressing here
                    // TODO: implement adding backup entry to db after successful backup
                    var serverConfig = dbContext.Configurations.FirstOrDefault(x => x.ServerId == db.GetServerId());
                    if (serverConfig is null)
                    {
                        logger.LogWarning("Can't find any backups configuration for server {ServerId}", db.GetServerId());
                        continue;
                    }
                    await db.PerformBackup(serverConfig);
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