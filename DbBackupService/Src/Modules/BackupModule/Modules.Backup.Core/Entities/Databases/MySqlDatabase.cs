using Microsoft.Extensions.Logging;
using Modules.Backup.Core.Entities.DbContext;
using MySql.Data.MySqlClient;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class MySqlDatabase(DbServerConnection serverConnection, ILogger logger)
    : DatabaseBase(serverConnection, logger)
{
    private readonly DbServerConnection _serverConnection = serverConnection;

    protected override async Task PerformBackupInternal(string fullFilePath)
    {
        var connectionString =
            $"Server={_serverConnection.ServerHost};Port={_serverConnection.ServerPort};Database={_serverConnection.DbName};Uid={_serverConnection.DbUser};Pwd={_serverConnection.DbPasswd};";

        await using var connection = new MySqlConnection(connectionString);
        await using var cmd = new MySqlCommand();
        using var backup = new MySqlBackup(cmd);

        cmd.Connection = connection;
        await connection.OpenAsync();

        backup.ExportToFile(fullFilePath);

        await connection.CloseAsync();
    }
}