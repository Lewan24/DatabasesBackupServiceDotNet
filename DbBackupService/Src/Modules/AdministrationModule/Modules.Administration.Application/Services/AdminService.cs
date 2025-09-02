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
            IsEmailConfirmed = x.EmailConfirmed,
            Roles = userManager.GetRolesAsync(x).Result
        }).ToList();
        
        return Task.FromResult(users);
    }

    public async Task<OneOf<Success, string>> ToggleUserBlockade(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "User Id cannot be null or empty.";

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return "Can't find specified user";

        var isAdmin = await IsUserAdmin(user.UserName);
        if (isAdmin is not null && isAdmin.Value)
            return "Can't change blockade for admin user";
        
        // TODO: Updating doesnt work // need to check why
        user.EmailConfirmed = !user.EmailConfirmed;
        var updateResult = await userManager.UpdateAsync(user);
        
        if (updateResult.Errors.Any())
            return updateResult.Errors.First().Description;
        
        return new Success();
    }
}