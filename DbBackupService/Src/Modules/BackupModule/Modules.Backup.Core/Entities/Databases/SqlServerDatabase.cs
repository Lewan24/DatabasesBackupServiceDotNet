using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Core.Entities.Databases;

public sealed class SqlServerDatabase(DbServerConnection serverConnection, DbServerTunnel serverTunnel, ILogger logger)
    : DatabaseBase(serverConnection, serverTunnel, logger)
{
    private readonly DbServerConnection _serverConnection = serverConnection;

    protected override async Task PerformBackupInternal(string fullFilePath)
    {
        var host = _serverConnection.IsTunnelRequired ? "127.0.0.1" : _serverConnection.ServerHost;
        var port = _serverConnection.IsTunnelRequired ? serverTunnel.LocalPort : _serverConnection.ServerPort;

        await Task.Run(() =>
        {
            try
            {
                var serverName = _serverConnection.IsTunnelRequired
                    ? $"127.0.0.1,{serverTunnel.LocalPort}"
                    : $"{_serverConnection.ServerHost},{_serverConnection.ServerPort}";

                var connection = new ServerConnection(serverName, _serverConnection.DbUser, _serverConnection.DbPasswd)
                {
                    LoginSecure = false,
                    StatementTimeout = 0
                };

                var server = new Server(connection);
                var db = server.Databases[_serverConnection.DbName];

                if (db == null)
                    throw new Exception($"Database '{_serverConnection.DbName}' not found on server '{host}:{port}'");

                var backup = new Microsoft.SqlServer.Management.Smo.Backup
                {
                    Database = db.Name,
                    Action = BackupActionType.Database,
                    Initialize = true
                };

                backup.Devices.AddDevice(fullFilePath, DeviceType.File);
                backup.SqlBackup(server);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while performing MSSQL backup for {DatabaseName}", _serverConnection.DbName);
                throw;
            }
        });
    }
}
