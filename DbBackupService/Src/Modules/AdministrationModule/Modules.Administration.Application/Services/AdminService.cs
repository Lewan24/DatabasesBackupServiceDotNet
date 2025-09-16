using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Auth.Core.Entities;
using Modules.Auth.Infrastructure.DbContexts;
using Modules.Auth.Shared.ActionsRequests;
using Modules.Auth.Shared.Dtos;
using Modules.Auth.Shared.Static.Entities;
using OneOf;
using OneOf.Types;

namespace Modules.Administration.Application.Services;

public class AdminService(
    AppIdentityDbContext context,
    UserManager<AppUser> userManager)
{
    public async Task<bool?> IsUserAdmin(string? identityName)
    {
        if (string.IsNullOrWhiteSpace(identityName))
            return null;

        var user = context.Users.SingleOrDefault(u => u.UserName == identityName);

        if (user is null)
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
                IsEmailConfirmed = x.EmailConfirmed,
                Roles = userManager.GetRolesAsync(x).Result
            }).ToList();

        return Task.FromResult(users);
    }

    public async Task<OneOf<Success, string>> ToggleUserBlockade(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "User Id cannot be null or empty.";

        var user = context.Users.AsTracking().SingleOrDefault(u => u.Id == userId);

        if (user is null)
            return "Can't find specified user";

        var isAdmin = await IsUserAdmin(user.UserName);
        if (isAdmin is not null && isAdmin.Value)
            return "Can't change blockade for admin user";

        user.IsBlocked = !user.IsBlocked;
        await context.SaveChangesAsync();

        return new Success();
    }

    public async Task<OneOf<Success, string>> EditUser(EditUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Id))
            return "Invalid Request. Check data and try again.";

        var user = await userManager.FindByIdAsync(request.Id);

        if (user is null)
            return "Can't find specified user";

        await userManager.SetEmailAsync(user, request.Email);
        await userManager.SetUserNameAsync(user, request.Email);

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            var confirmEmailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, confirmEmailToken);
        }

        if (string.IsNullOrWhiteSpace(request.Password))
            return new Success();

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword) ||
            request.ConfirmPassword != request.Password)
            return "Password do not match";

        var resetPassToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetPassResult = await userManager.ResetPasswordAsync(user, resetPassToken, request.Password);

        if (resetPassResult.Errors.Any())
            return resetPassResult.Errors.First().Description;

        return new Success();
    }
}