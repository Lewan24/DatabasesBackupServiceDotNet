using Application.Data.Interfaces;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using NLog;

namespace Application.Data.Services;

public class DbBackupService : IDbBackupService
{
    private readonly Logger _logger;
    private readonly ApplicationConfiguration _appConfig;
    private int _madeBackupsCounter;
    
    public DbBackupService(Logger logger, ApplicationConfiguration appConfig)
    {
        _appConfig = appConfig;
        _logger = logger.Factory.GetLogger(nameof(DbBackupService));
    }
    
    public async Task RunService(List<DatabaseConfigModel> dbConfigurations)
    {
        _logger.Info("{ServiceName} has started", nameof(DbBackupService));
        _logger.Info("Preparing backups for {DbCConfigsCount}...", dbConfigurations.Count);
        
        await PerformBackup(dbConfigurations);
        
        _logger.Info("{ServiceName} has finished its work", nameof(DbBackupService));
    }

    private async Task PerformBackup(List<DatabaseConfigModel> dbConfigurations)
    {
        try
        {
            var databasesList = new List<IDatabase>();

            foreach (var dbConfig in dbConfigurations)
            {
                IDatabase database = dbConfig.DbType switch
                {
                    DatabaseType.MySql => new MySqlDatabase(dbConfig, _logger, _appConfig),
                    DatabaseType.PostgreSql => new PostgreSqlDatabase(dbConfig, _logger, _appConfig),
                    _ => throw new NotSupportedException(
                        $"Selected type of database is not supported: {dbConfig.DbType}")
                };

                databasesList.Add(database);
            }

            foreach (var db in databasesList)
            {
                var result = await db.PerformBackup();
                
                if (result)
                    _madeBackupsCounter++;
            }
        }
        catch (NotSupportedException e)
        {
            _logger.Warn(e, "Exception thrown while preparing databases");
        }
        catch (Exception e)
        {
            _logger.Warn(e, "Exception thrown while performing backup in database");
        }
    }
    
    public Task<int> GetBackupsCounter() => Task.FromResult(_madeBackupsCounter);
}