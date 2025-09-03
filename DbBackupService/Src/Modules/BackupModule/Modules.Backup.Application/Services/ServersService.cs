using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Auth.Core.Entities;
using Modules.Auth.Shared.Static.Entities;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Infrastructure.DbContexts;
using Modules.Backup.Shared.Dtos;
using OneOf;
using OneOf.Types;

namespace Modules.Backup.Application.Services;

public class ServersService (
    BackupsDbContext db,
    UserManager<AppUser> userManager)
{
    public async Task<OneOf<List<ServerConnectionDto>, string>> GetServers(string? identityName)
    {
        if (string.IsNullOrWhiteSpace(identityName))
            return "Can't access user name";
        
        var user = await userManager.FindByNameAsync(identityName);
        if (user is null)
            return "Can't find user";
        
        var userServers = db.UsersServers
            .AsNoTracking()
            .Where(x => x.UserId == Guid.Parse(user.Id))
            .Select(x => x.ServerId)
            .ToList();

        if (userServers.Count == 0)
            return new List<ServerConnectionDto>();
        
        var dbServers = db.DbConnections
            .AsNoTracking()
            .Where(x => userServers.Contains(x.Id))
            .ToList();

        var isAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);
        if (!isAdmin)
            dbServers.RemoveAll(x => x.IsDisabled);

        return dbServers
            .Select(x => new ServerConnectionDto
            {
                Id = x.Id,
                ConnectionName = x.ConnectionName,
                ServerHost = x.ServerHost,
                ServerPort = x.ServerPort,
                DbName = x.DbName,
                DbType = x.DbType,
                DbUser = x.DbUser,
                IsTunnelRequired = x.IsTunnelRequired
            })
            .ToList();;
    }

    public async Task<OneOf<Success, string>> CreateServer(ServerConnectionDto newServer, string? identityName)
{
    // 1. Walidacja podstawowych danych
    if (string.IsNullOrWhiteSpace(newServer.ConnectionName))
        return "Connection name is required";
    if (string.IsNullOrWhiteSpace(newServer.ServerHost))
        return "Server host is required";
    if (newServer.ServerPort <= 0)
        return "Invalid server port";
    if (string.IsNullOrWhiteSpace(newServer.DbName))
        return "Database name is required";
    if (string.IsNullOrWhiteSpace(newServer.DbUser))
        return "Database user is required";

    // 2. Tworzymy obiekt encji serwera
    var dbServer = new DbServerConnection
    {
        Id = Guid.CreateVersion7(),
        ConnectionName = newServer.ConnectionName,
        ServerHost = newServer.ServerHost,
        ServerPort = newServer.ServerPort,
        DbName = newServer.DbName,
        DbType = newServer.DbType,
        DbUser = newServer.DbUser,
        DbPasswd = newServer.DbPasswd ?? string.Empty,
        IsTunnelRequired = newServer.IsTunnelRequired,
        IsDisabled = false
    };

    // 3. Obsługa tunelu (opcjonalna)
    if (newServer.IsTunnelRequired)
    {
        if (newServer.Tunnel is null)
            return "Tunnel configuration required";

        if (string.IsNullOrWhiteSpace(newServer.Tunnel.ServerHost))
            return "Tunnel host is required";
        if (string.IsNullOrWhiteSpace(newServer.Tunnel.Username))
            return "Tunnel username is required";
        if (newServer.Tunnel.LocalPort is <= 0 or > 65535)
            return "Invalid tunnel local port";
        if (string.IsNullOrWhiteSpace(newServer.Tunnel.RemoteHost))
            return "Tunnel remote host is required";
        if (newServer.Tunnel.RemotePort is <= 0 or > 65535)
            return "Invalid tunnel remote port";

        var dbTunnel = new DbServerTunnel
        {
            Id = Guid.CreateVersion7(),
            ServerHost = newServer.Tunnel.ServerHost!,
            SshPort = newServer.Tunnel.SshPort,
            Username = newServer.Tunnel.Username!,
            UsePasswordAuth = newServer.Tunnel.UsePasswordAuth,
            Password = newServer.Tunnel.Password,
            PrivateKeyContent = newServer.Tunnel.PrivateKeyContent,
            LocalPort = newServer.Tunnel.LocalPort,
            RemoteHost = newServer.Tunnel.RemoteHost!,
            RemotePort = newServer.Tunnel.RemotePort,
            Description = newServer.Tunnel.Description,
            IsActive = true
        };
        
        // zapisujemy tunel
        db.DbServerTunnels.Add(dbTunnel);
        dbServer.TunnelId = dbTunnel.Id;
    }

    // 4. Zapis serwera
    db.DbConnections.Add(dbServer);

    if (string.IsNullOrWhiteSpace(identityName))
        return "Can't access username";
    
    var user = await userManager.FindByNameAsync(identityName);
    
    if (user is null)
        return "Can't find user";
    
    var isAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);
    
    if (!isAdmin)
        db.UsersServers.Add(new ServersUsers
        {
            UserId = Guid.Parse(user.Id),
            ServerId = dbServer.Id
        });
    
    await db.SaveChangesAsync();

    return new Success();
}

public async Task<OneOf<Success, string>> EditServer(ServerConnectionDto server)
{
    // 1. Walidacja podstawowych danych (podobnie jak w CreateServer)
    if (string.IsNullOrWhiteSpace(server.ConnectionName))
        return "Connection name is required";
    if (string.IsNullOrWhiteSpace(server.ServerHost))
        return "Server host is required";
    if (server.ServerPort <= 0)
        return "Invalid server port";
    if (string.IsNullOrWhiteSpace(server.DbName))
        return "Database name is required";
    if (string.IsNullOrWhiteSpace(server.DbUser))
        return "Database user is required";

    // 2. Szukamy istniejącego serwera
    var dbServer = await db.DbConnections
        .FirstOrDefaultAsync(s => s.Id == server.Id);

    if (dbServer is null)
        return $"Server with id {server.Id} not found";

    // 3. Aktualizacja danych serwera
    dbServer.ConnectionName = server.ConnectionName;
    dbServer.ServerHost = server.ServerHost;
    dbServer.ServerPort = server.ServerPort;
    dbServer.DbName = server.DbName;
    dbServer.DbType = server.DbType;
    dbServer.DbUser = server.DbUser;
    dbServer.DbPasswd = server.DbPasswd ?? string.Empty;
    dbServer.IsTunnelRequired = server.IsTunnelRequired;

    // 4. Obsługa tunelu
    if (server.IsTunnelRequired)
    {
        if (server.Tunnel is null)
            return "Tunnel configuration required";

        var dbTunnel = await db.DbServerTunnels
            .FirstOrDefaultAsync(t => t.Id == server.Tunnel.Id);

        if (dbTunnel is null)
        {
            // jeśli tunel nie istnieje, tworzymy nowy
            dbTunnel = new DbServerTunnel
            {
                Id = server.Tunnel.Id,
                IsActive = true,
                ServerHost = "",
                Username = "",
                RemoteHost = ""
            };
            
            db.DbServerTunnels.Add(dbTunnel);
        }

        // aktualizacja wartości tunelu
        dbTunnel.ServerHost = server.Tunnel.ServerHost!;
        dbTunnel.SshPort = server.Tunnel.SshPort;
        dbTunnel.Username = server.Tunnel.Username!;
        dbTunnel.UsePasswordAuth = server.Tunnel.UsePasswordAuth;
        dbTunnel.Password = server.Tunnel.Password;
        dbTunnel.PrivateKeyContent = server.Tunnel.PrivateKeyContent;
        dbTunnel.LocalPort = server.Tunnel.LocalPort;
        dbTunnel.RemoteHost = server.Tunnel.RemoteHost!;
        dbTunnel.RemotePort = server.Tunnel.RemotePort;
        dbTunnel.Description = server.Tunnel.Description;

        dbServer.TunnelId = dbTunnel.Id;
    }

    await db.SaveChangesAsync();

    return new Success();
}

}