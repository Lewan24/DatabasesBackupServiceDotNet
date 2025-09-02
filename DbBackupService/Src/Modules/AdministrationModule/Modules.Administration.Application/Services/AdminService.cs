using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Auth.Core.Entities;
using Modules.Auth.Infrastructure.DbContexts;
using Modules.Auth.Shared.Dtos;
using Modules.Auth.Shared.Static.Entities;
using OneOf;
using OneOf.Types;

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

    public Task<List<UserDto>> GetUsersList()
    {
        var users = context.Users
            .AsNoTracking()
            .Select(x => new UserDto
        {
            Id = Guid.Parse(x.Id),
            Email = x.Email!,
            IsBlocked = x.IsBlocked,
            Roles = userManager.GetRolesAsync(x).Result
        }).ToList();
        
        return Task.FromResult(users);
    }

    public async Task<OneOf<Success, string>> ToggleUserBlockade(string userId)
    {
        logger.LogInformation("Trying to toggle user blockade...");
        
        if (string.IsNullOrWhiteSpace(userId))
            return "User Id cannot be null or empty.";

        logger.LogInformation("Finding user...");
        var user = context.Users.AsTracking().SingleOrDefault(u => u.Id == userId);

        if (user is null)
            return "Can't find specified user";
        
        logger.LogInformation("User's blockade status: {BlockadeStatus}",  user.IsBlocked);
        var isAdmin = await IsUserAdmin(user.UserName);
        if (isAdmin is not null && isAdmin.Value)
            return "Can't change blockade for admin user";
        
        logger.LogInformation("Changing IsBlocked status...");
        user.IsBlocked = !user.IsBlocked;
        var rowsAffected = await context.SaveChangesAsync();
        logger.LogInformation("Rows affected: {RowsAffected}",  rowsAffected);
        logger.LogInformation("User's new blockade status: {BlockadeStatus}",  user.IsBlocked);
        
        return new Success();
    }
}