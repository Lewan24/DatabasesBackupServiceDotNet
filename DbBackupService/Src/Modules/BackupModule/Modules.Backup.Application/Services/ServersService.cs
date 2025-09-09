using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Auth.Core.Entities;
using Modules.Auth.Infrastructure.DbContexts;
using Modules.Auth.Shared.Static.Entities;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Backup.Infrastructure.DbContexts;
using Modules.Backup.Shared.Dtos;
using Modules.Backup.Shared.Requests;
using OneOf;
using OneOf.Types;

namespace Modules.Backup.Application.Services;

public class ServersService (
    BackupsDbContext db,
    AppIdentityDbContext appIdentityDbContext,
    UserManager<AppUser> userManager,
    ILogger<ServersService> logger,
    NotifyService notifyService)
{
    public async Task<OneOf<List<ServerConnectionDto>, string>> GetServers(string? identityName)
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
            var userServers = db.UsersServers
                .AsNoTracking()
                .Where(x => x.UserId == Guid.Parse(user.Id))
                .Select(x => x.ServerId)
                .ToList();

            if (userServers.Count == 0)
                return new List<ServerConnectionDto>();
        
            dbServers = db.DbConnections
                .AsNoTracking()
                .Where(x => userServers.Contains(x.Id))
                .ToList();
            
            dbServers.RemoveAll(x => x.IsDisabled);
        }
        else
        {
            dbServers = db.DbConnections
                .AsNoTracking()
                .ToList();
        }

        var dtoServers = new List<ServerConnectionDto>();
        
        foreach (var server in dbServers)
        {
            var dto = new ServerConnectionDto
            {
                Id = server.Id,
                ConnectionName = server.ConnectionName,
                ServerHost = server.ServerHost,
                ServerPort = server.ServerPort,
                DbName = server.DbName,
                DbType = server.DbType,
                DbUser = server.DbUser,
                IsTunnelRequired = server.IsTunnelRequired
            };
            
            var dbTunnel = db.DbServerTunnels.FirstOrDefault(x => x.Id == server.TunnelId);

            if (dbTunnel is not null)
            {
                dto.Tunnel = new ServerTunnelDto
                {
                    Id = dbTunnel.Id,
                    ServerHost = dbTunnel.ServerHost,
                    SshPort = dbTunnel.SshPort,
                    Username = dbTunnel.Username,
                    LocalPort = dbTunnel.LocalPort,
                    RemoteHost = dbTunnel.RemoteHost,
                    RemotePort = dbTunnel.RemotePort,
                    Description = dbTunnel.Description
                };
            }
            
            dtoServers.Add(dto);
        }

        return dtoServers;
    }
    
    public async Task<OneOf<List<ServerNameIdDto>, string>> GetAvailableServersBasic(string? identityName)
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
            var userServers = db.UsersServers
                .AsNoTracking()
                .Where(x => x.UserId == Guid.Parse(user.Id))
                .Select(x => x.ServerId)
                .ToList();

            if (userServers.Count == 0)
                return new List<ServerNameIdDto>();
        
            dbServers = db.DbConnections
                .AsNoTracking()
                .Where(x => userServers.Contains(x.Id))
                .ToList();
            
            dbServers.RemoveAll(x => x.IsDisabled);
        }
        else
        {
            dbServers = db.DbConnections
                .AsNoTracking()
                .ToList();
        }

        var dtoServers = dbServers
            .Select(x => new ServerNameIdDto(x.Id, x.ConnectionName))
            .ToList();
        
        return dtoServers;
    }

    public async Task<OneOf<Success, string>> CreateServer(ServerConnectionDto newServer, string? identityName)
    {
        List<string> errors = new();
        
        if (string.IsNullOrWhiteSpace(newServer.ConnectionName))
            errors.Add("Connection name is required");
        if (string.IsNullOrWhiteSpace(newServer.ServerHost))
            errors.Add("Server host is required");
        if (newServer.ServerPort <= 0)
            errors.Add("Invalid server port");
        if (string.IsNullOrWhiteSpace(newServer.DbName))
            errors.Add("Database name is required");
        if (string.IsNullOrWhiteSpace(newServer.DbUser))
            errors.Add("Database user is required");
        if (string.IsNullOrWhiteSpace(newServer.DbPasswd))
            errors.Add("Database password is required");

        if (newServer.IsTunnelRequired)
        {
            if (newServer.Tunnel is null)
                errors.Add("Tunnel configuration required");

            if (newServer.Tunnel is not null)
            {
                if (string.IsNullOrWhiteSpace(newServer.Tunnel.ServerHost))
                    errors.Add("Tunnel host is required");
                if (string.IsNullOrWhiteSpace(newServer.Tunnel.Username))
                    errors.Add("Tunnel username is required");

                if (string.IsNullOrWhiteSpace(newServer.Tunnel.PrivateKeyContent))
                    if (string.IsNullOrWhiteSpace(newServer.Tunnel.Password))
                        errors.Add("Tunnel password is required");
                
                if (newServer.Tunnel.LocalPort is <= 0 or > 65535)
                    errors.Add("Invalid tunnel local port");
                if (string.IsNullOrWhiteSpace(newServer.Tunnel.RemoteHost))
                    errors.Add("Tunnel remote host is required");
                if (newServer.Tunnel.RemotePort is <= 0 or > 65535)
                    errors.Add("Invalid tunnel remote port");
            }
        }
        
        if (errors.Any())
            return string.Join(", ", errors);

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
            DbPasswd = newServer.DbPasswd!,
            IsTunnelRequired = newServer.IsTunnelRequired,
            IsDisabled = false
        };

        // 3. Obsługa tunelu (opcjonalna)
        if (newServer.IsTunnelRequired)
        {
            var dbTunnel = new DbServerTunnel
            {
                Id = Guid.CreateVersion7(),
                ServerHost = newServer.Tunnel!.ServerHost!,
                SshPort = newServer.Tunnel.SshPort,
                Username = newServer.Tunnel.Username!,
                UsePasswordAuth = string.IsNullOrWhiteSpace(newServer.Tunnel.PrivateKeyContent),
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

        var serverUser = new ServersUsers
        {
            UserId = Guid.Parse(user.Id),
            ServerId = dbServer.Id
        };
        
        if (!isAdmin)
            db.UsersServers.Add(serverUser);
        
        await db.SaveChangesAsync();

        await notifyService.CallServerCreatedEvent(user.UserName!);
        
        return new Success();
    }

    public async Task<OneOf<Success, string>> EditServer(ServerConnectionDto server)
    {
        // TODO: Implement checking if user has access to server or is admin
        List<string> errors = new();
        
        if (string.IsNullOrWhiteSpace(server.ConnectionName))
            errors.Add("Connection name is required");
        if (string.IsNullOrWhiteSpace(server.ServerHost))
            errors.Add("Server host is required");
        if (server.ServerPort <= 0)
            errors.Add("Invalid server port");
        if (string.IsNullOrWhiteSpace(server.DbName))
            errors.Add("Database name is required");
        if (string.IsNullOrWhiteSpace(server.DbUser))
            errors.Add("Database user is required");

        if (server.IsTunnelRequired)
        {
            if (server.Tunnel is null)
                errors.Add("Tunnel configuration required");

            if (server.Tunnel is not null)
            {
                if (string.IsNullOrWhiteSpace(server.Tunnel.ServerHost))
                    errors.Add("Tunnel host is required");
                if (string.IsNullOrWhiteSpace(server.Tunnel.Username))
                    errors.Add("Tunnel username is required");
                
                if (string.IsNullOrWhiteSpace(server.Tunnel.PrivateKeyContent))
                    if (string.IsNullOrWhiteSpace(server.Tunnel.Password))
                        errors.Add("Tunnel password is required");
                
                if (server.Tunnel.LocalPort is <= 0 or > 65535)
                    errors.Add("Invalid tunnel local port");
                if (string.IsNullOrWhiteSpace(server.Tunnel.RemoteHost))
                    errors.Add("Tunnel remote host is required");
                if (server.Tunnel.RemotePort is <= 0 or > 65535)
                    errors.Add("Invalid tunnel remote port");
            }
        }

        if (errors.Any())
            return string.Join(", ", errors);
        
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
        
        if (!string.IsNullOrWhiteSpace(server.DbPasswd))
            dbServer.DbPasswd = server.DbPasswd;
        
        dbServer.IsTunnelRequired = server.IsTunnelRequired;

        // 4. Obsługa tunelu
        if (server.IsTunnelRequired)
        {
            var dbTunnel = await db.DbServerTunnels
                .FirstOrDefaultAsync(t => t.Id == server.Tunnel!.Id);

            if (dbTunnel is null)
            {
                // jeśli tunel nie istnieje, tworzymy nowy
                dbTunnel = new DbServerTunnel
                {
                    Id = Guid.CreateVersion7(),
                    IsActive = true,
                    ServerHost = "",
                    Username = "",
                    RemoteHost = ""
                };
                
                db.DbServerTunnels.Add(dbTunnel);
            }

            // aktualizacja wartości tunelu
            dbTunnel.ServerHost = server.Tunnel!.ServerHost!;
            dbTunnel.SshPort = server.Tunnel.SshPort;
            dbTunnel.Username = server.Tunnel.Username!;
            dbTunnel.UsePasswordAuth = string.IsNullOrWhiteSpace(server.Tunnel.PrivateKeyContent);
            dbTunnel.Password = server.Tunnel.Password;
            dbTunnel.PrivateKeyContent = server.Tunnel.PrivateKeyContent;
            dbTunnel.LocalPort = server.Tunnel.LocalPort;
            dbTunnel.RemoteHost = server.Tunnel.RemoteHost!;
            dbTunnel.RemotePort = server.Tunnel.RemotePort;
            dbTunnel.Description = server.Tunnel.Description;

            dbServer.TunnelId = dbTunnel.Id;
        }

        await db.SaveChangesAsync();

        await notifyService.CallServerHasChangedEvent(dbServer.Id);
        
        return new Success();
    }

    public async Task<OneOf<Success, string>> ToggleDisabledStatus(Guid serverId, string? username)
    {
        if (serverId == Guid.Empty)
            return "Id can't be empty";

        var server = await db.DbConnections.FirstOrDefaultAsync(x => x.Id == serverId);

        if (server is null)
            return "Can't find specified server";

        if (string.IsNullOrWhiteSpace(username))
            return "Can't access username";
        
        var user = await userManager.FindByNameAsync(username);
        if (user is null)
            return "Can't find user";
        
        var isAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);
        if (isAdmin)
            server.IsDisabled = !server.IsDisabled;
        else
        {
            var access = await db.UsersServers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == Guid.Parse(user.Id) && x.ServerId == serverId);

            if (access is null)
                return "User does not have required access to this server";
            
            server.IsDisabled = true;
        }
        
        await db.SaveChangesAsync();
        
        if (server.IsDisabled)
            await notifyService.CallServerHasChangedEvent(server.Id);
        else
        {
            var usersWithAccess = db.UsersServers
                .AsNoTracking()
                .Where(x => x.ServerId == serverId)
                .ToList();

            foreach (var userAccess in usersWithAccess)
            {
                var tempUser = await userManager.FindByIdAsync(userAccess.UserId.ToString());
                await notifyService.CallServerCreatedEvent(tempUser?.UserName!);
            }
        }
        
        return new Success();
    }

    public Task<List<ServersUsersListDto>> GetServersUsers()
    {
        var servers = db.DbConnections
            .AsNoTracking()
            .Select(x => new ServersUsersListDto
            {
                ServerId = x.Id, 
                IsServerDisabled = x.IsDisabled, 
                ServerConnectionName = x.ConnectionName
            }).ToList();

        foreach (var server in servers)
        {
            var usersWithAccess = db.UsersServers
                .Count(x => x.ServerId == server.ServerId);
            
            server.UsersWithAccess = usersWithAccess;
        }

        return Task.FromResult(servers);
    }

    public Task<OneOf<List<string>, string>> GetUsersThatAccessServer(Guid serverId)
    {
        if (serverId == Guid.Empty)
            return Task.FromResult<OneOf<List<string>, string>>("Id cant be empty");
        
        try
        {
            var serversUsersIds = db.UsersServers
                .AsNoTracking()
                .Where(x => x.ServerId == serverId)
                .Select(x => x.UserId)
                .ToList();

            var usersEmails = new List<string>();
            
            foreach (var userId in serversUsersIds)
                usersEmails.Add(appIdentityDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId.ToString()).Result?.Email!);
            
            return Task.FromResult<OneOf<List<string>, string>>(usersEmails!);
        }
        catch (Exception e)
        {
            return Task.FromResult<OneOf<List<string>, string>>(e.Message);
        }
    }

    public async Task<OneOf<List<string>, string>> GetAllUsersThatDoesNotHaveAccessToServer(Guid serverId)
    {
        if (serverId == Guid.Empty)
            return "Id cant be empty";

        try
        {
            var serversUsersIds = await db.UsersServers
                .AsNoTracking()
                .Where(x => x.ServerId == serverId)
                .Select(x => x.UserId)
                .ToListAsync();

            var allUsers = await appIdentityDbContext.Users
                .AsNoTracking()
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            var usersWithoutAccess = new List<string>();
            
            foreach (var user in allUsers)
                if (!serversUsersIds.Contains(Guid.Parse(user.Id)))
                    usersWithoutAccess.Add(user.Email!);

            return usersWithoutAccess;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }


    public async Task<OneOf<Success, string>> RemoveUserAccessFromServer(ModifyServerAccessRequest request)
    {
        if (request.ServerId == Guid.Empty || string.IsNullOrWhiteSpace(request.UserEmail))
            return "Invalid request";
        
        var user = await userManager.FindByEmailAsync(request.UserEmail);
        if (user is null)
            return "Can't find user";

        var serverUserAccess = await db.UsersServers
            .FirstOrDefaultAsync(x => x.ServerId == request.ServerId && x.UserId == Guid.Parse(user.Id));

        if (serverUserAccess is null)
            return "Can't find specified user access";

        db.UsersServers.Remove(serverUserAccess);
        await db.SaveChangesAsync();

        await notifyService.CallServerHasChangedEvent(request.ServerId);
        
        return new Success();
    }

    public async Task<OneOf<Success, string>> GiveUserAccessToServer(ModifyServerAccessRequest request)
    {
        if (request.ServerId == Guid.Empty || string.IsNullOrWhiteSpace(request.UserEmail))
            return "Invalid request";
        
        var user = await userManager.FindByEmailAsync(request.UserEmail);
        if (user is null)
            return "Can't find user";

        var newServerUser = new ServersUsers
        {
            ServerId = request.ServerId,
            UserId = Guid.Parse(user.Id)
        };

        db.UsersServers.Add(newServerUser);
        await db.SaveChangesAsync();

        await notifyService.CallServerCreatedEvent(user.UserName!);
        
        return new Success();
    }
}