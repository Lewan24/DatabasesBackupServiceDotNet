using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Auth.Core.Entities;
using Modules.Auth.Shared.Static.Entities;
using Modules.Backup.Application.Interfaces;
using Modules.Backup.Core.Entities.Databases;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Core.Interfaces;
using Modules.Backup.Core.StaticClasses;
using Modules.Backup.Infrastructure.DbContexts;
using Modules.Backup.Shared.Dtos;
using Modules.Backup.Shared.Enums;
using OneOf;
using OneOf.Types;

namespace Modules.Backup.Application.Services;

internal sealed class DbBackupService(
    BackupsDbContext dbContext,
    UserManager<AppUser> userManager,
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
        
        var result = await PerformBackup([server]);

        return result.Match<OneOf<Success, string>>(
            _ =>
            {
                logger.LogInformation("{ServiceName} finished its job.", nameof(DbBackupService));
                return new Success();
            },
            error => error
        );
    }

    private async Task<OneOf<Success, string>> PerformBackup(List<DbServerConnection> serversConnections)
    {
        try
        {
            var databasesList = new List<IDatabase>();

            foreach (var serverConn in serversConnections)
            {
                var serverTunnel = dbContext.DbServerTunnels.FirstOrDefault(x => x.Id == serverConn.TunnelId);
                
                IDatabase database = serverConn.DbType switch
                {
                    DatabaseType.MySql => new MySqlDatabase(serverConn, serverTunnel!, logger),
                    DatabaseType.PostgreSql => new PostgreSqlDatabase(serverConn, serverTunnel!, logger),
                    DatabaseType.SqlServer => new SqlServerDatabase(serverConn, serverTunnel!, logger),
                    _ => throw new NotSupportedException(
                        $"Selected type of database is not supported: {serverConn.DbType}")
                };

                databasesList.Add(database);
            }

            foreach (var db in databasesList)
                try
                {
                    // TODO: implement removing db entries on successful clearing old backups
                    var serverConfig = dbContext.Configurations.FirstOrDefault(x => x.ServerId == db.GetServerId());
                    if (serverConfig is null)
                    {
                        logger.LogWarning("Can't find any backups configuration for server {ServerId}", db.GetServerId());
                        continue;
                    }
                    
                    var backupPath = serverConfig.CreateBackupPath(db.GetServerConnection(), null);
                    await DeleteOldBackup.Delete(backupPath.DirectoryPath, serverConfig.TimeInDaysToHoldBackups, logger);

                    var serverConn = db.GetServerConnection();
                    var newDbBackup = new PerformedBackup
                    {
                        Id = Guid.CreateVersion7(),
                        CreatedOn = DateTime.Now,
                        ServerConnectionId = db.GetServerId(),
                        Name = $"{serverConn.ServerHost}:{serverConn.ServerPort}_{serverConn.DbName}",
                        FilePath = ""
                    };
                    
                    var createdFileName = await db.PerformBackup(serverConfig);
                    newDbBackup.FilePath = Path.Combine(backupPath.DirectoryPath, createdFileName);

                    string compressedFilename = "";
                    
                    if (serverConn.DbType is not DatabaseType.SqlServer)
                        compressedFilename = CompressBackupFile.Perform(backupPath.DirectoryPath, createdFileName);
                    
                    dbContext.Backups.Add(newDbBackup);
                    await dbContext.SaveChangesAsync();
                    
                    if (!string.IsNullOrWhiteSpace(compressedFilename))
                        logger.LogInformation("Successfully created and compressed backup: [{Database}], [{ZipFile}]", db.GetDatabaseName(), compressedFilename);
                    
                    return new Success();
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Exception thrown while performing backup in database");
                    return e.Message;
                }
        }
        catch (NotSupportedException e)
        {
            logger.LogWarning(e, "Exception thrown while preparing databases");
            return e.Message;
        }
        
        return new Success();
    }
    
    public async Task<OneOf<List<PerformedBackupDto>, string>> GetAllBackups(string? identityName)
    {
        if (string.IsNullOrWhiteSpace(identityName))
            return "Can't access user name";
        
        var user = await userManager.FindByNameAsync(identityName);
        if (user is null)
            return "Can't find user";
        
        var isAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);

        List<DbServerConnection> dbServers;

        if (!isAdmin)
        {
            var userServers = dbContext.UsersServers
                .AsNoTracking()
                .Where(x => x.UserId == Guid.Parse(user.Id))
                .Select(x => x.ServerId)
                .ToList();

            if (userServers.Count == 0)
                return new List<PerformedBackupDto>();
        
            dbServers = dbContext.DbConnections
                .AsNoTracking()
                .Where(x => userServers.Contains(x.Id))
                .ToList();
            
            dbServers.RemoveAll(x => x.IsDisabled);
        }
        else
        {
            dbServers = dbContext.DbConnections
                .AsNoTracking()
                .ToList();
        }

        var backups = new List<PerformedBackupDto>();

        if (isAdmin)
            return dbContext.Backups
                .AsNoTracking()
                .Select(x => new PerformedBackupDto
                {
                    Id = x.Id, 
                    CreatedOn = x.CreatedOn, 
                    FilePath = x.FilePath, 
                    Name = x.Name, 
                    ServerConnectionId = x.ServerConnectionId,
                    Test = dbContext.BackupsTests.AsNoTracking().Where(y => y.BackupId == x.Id).Select(y => new BackupTestDto
                    {
                        Id = y.Id, 
                        ErrorMessage = y.ErrorMessage, 
                        IsSuccess = y.IsSuccess,
                        TestedOn = y.TestedOn
                    }).FirstOrDefault()
                }).ToList();

        foreach (var server in dbServers)
        {
            backups.AddRange(dbContext.Backups
                .AsNoTracking()
                .Where(x => x.ServerConnectionId == server.Id)
                .Select(x => new PerformedBackupDto
                {
                    Id = x.Id,
                    CreatedOn = x.CreatedOn,
                    FilePath = x.FilePath,
                    Name = x.Name,
                    ServerConnectionId = x.ServerConnectionId,
                    Test = dbContext.BackupsTests.AsNoTracking().Where(y => y.BackupId == x.Id).Select(y =>
                        new BackupTestDto
                        {
                            Id = y.Id,
                            ErrorMessage = y.ErrorMessage,
                            IsSuccess = y.IsSuccess,
                            TestedOn = y.TestedOn
                        }).FirstOrDefault()
                }));
        }
        
        return backups;
    }
}