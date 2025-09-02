using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Modules.Auth.Core.Entities;
using Modules.Auth.Infrastructure.DbContexts;
using Modules.Auth.Shared.Static.Entities;

namespace Modules.Administration.Application.Services;

public class AdminService (
    ILogger<AdminService> logger,
    AppIdentityDbContext context,
    UserManager<AppUser> userManager)
{
    public async Task<bool?> IsUserAdmin(string? identityName)
    {
        if (string.IsNullOrWhiteSpace(identityName))
            return null;
                
        var user = context.Users.SingleOrDefault(u => u.UserName == identityName);
        
        if  (user is null) 
            return null;

        var isUserAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);
        
        return isUserAdmin;
    }
}