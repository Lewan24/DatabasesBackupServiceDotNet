using Core.Interfaces;
using NLog;

namespace Core.Models;

public class MySqlDatabase : IDatabase
{
    private DatabaseConfigModel _config;
    private readonly Logger _logger;
    
    public MySqlDatabase(DatabaseConfigModel config, Logger logger)
    {
        _config = config;
        _logger = logger.Factory.GetLogger(nameof(MySqlDatabase));
    }
    
    public Task<bool> PerformBackup()
    {
        // TODO: Implement functionallity of creating backup for MySql
        _logger.Info("Performing backup for {DatabaseName}", _config.DbName);
        Console.WriteLine($"{_config.DbName} - backup completed");
        
        return Task.FromResult(true);
    }
}