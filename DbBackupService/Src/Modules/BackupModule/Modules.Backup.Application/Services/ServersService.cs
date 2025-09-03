using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Auth.Core.Entities;
using Modules.Auth.Shared.Static.Entities;
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
}