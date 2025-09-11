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
            var connectionString = PrepareConnectionString(serverConnection);

            await using var connection = new MySqlConnection(connectionString);
            await using var cmd = new MySqlCommand();
            using var backup = new MySqlBackup(cmd);

            cmd.Connection = connection;
            connection.Open();
            
            var fileName = $"{DateTime.Now:yyyy.MM.dd.HH.mm}.sql";
            var backupPath = serverConfig.CreateBackupPath(serverConnection, fileName);
            
            //backup.ExportToFile(backupPath.FullFilePath);
            //await connection.CloseAsync();

            return fileName;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while performing backup for {DatabaseName}", serverConnection.DbName);
            throw;
        }
    }

    private string PrepareConnectionString(DbServerConnection configuration)
    {
        var connectionString =
            $"Server={configuration.ServerHost};Port={configuration.ServerPort};Database={configuration.DbName};Uid={configuration.DbUser};Pwd={configuration.DbPasswd};";

        return connectionString;
    }
    
    public string GetDatabaseName() 
        => serverConnection.DbName;

    public Guid GetServerId()
        => serverConnection.Id;

    public DbServerConnection GetServerConnection()
        => serverConnection;
}