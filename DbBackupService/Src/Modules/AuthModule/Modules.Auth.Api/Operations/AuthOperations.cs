using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Auth.Api.Entities;
using Modules.Auth.Core.Entities;
using Modules.Auth.Infrastructure.DbContexts;
using Modules.Auth.Shared.ActionsRequests;
using Modules.Auth.Shared.Entities.Tokens;
using Modules.Auth.Shared.Interfaces.Token;
using Modules.Auth.Shared.Static.Entities;

namespace Modules.Auth.Api.Operations;

internal abstract record AuthOperations
{
    public static IResult Ping(HttpContext context, ILogger<AuthOperations> logger)
    {
        logger.LogInformation("Ping from {UserEmail}", context.User.Identity?.Name);
        return TypedResults.Ok("pong");
    }

    public static async Task<IResult> Login(
        HttpContext context,
        LoginRequest? request,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        AppIdentityDbContext db,
        ITokenValidationService tokenValidationService,
        IConfiguration config,
        ILogger<AuthOperations> logger)
    {
        var authSettings = config.GetSection("AuthSettings").Get<AuthSettings>() ?? new();

        logger.LogInformation("Trying to log {UserName} in...", request?.Email);
        if (!await CanLogIn(context, request, userManager, logger))
            return TypedResults.BadRequest("Nie można zalogować. Sprawdź poprawność danych.");

        var user = await userManager.FindByEmailAsync(request!.Email!);
        if (user is null)
            return TypedResults.NotFound("Taki użytkownik nie istnieje.");

        if (!user.EmailConfirmed)
            return TypedResults.BadRequest("Konto zablokowane.");

        await signInManager.SignInAsync(user, request.RememberMe);

        logger.LogInformation("User {UserName} successfully logged in. Generating token...", user.Email);
        var result = await GetUserToken(context, request, db, tokenValidationService, logger);
        if (string.IsNullOrWhiteSpace(result.Token))
            await CreateNewUserToken(db, request.Email!, authSettings.DefaultTokenExpirationTimeInMinutes, logger);

        return TypedResults.Accepted(request.Email);
    }

    public static async Task<IResult> ChangePassword(
        HttpContext context,
        ChangePasswordRequest request,
        UserManager<AppUser> userManager,
        ILogger<AuthOperations> logger)
    {
        logger.LogInformation("Changing password for {UserName}...", context.User.Identity?.Name);

        if (!IsValidChangePasswordRequest(request, out var error))
            return TypedResults.BadRequest(error);

        var user = await userManager.FindByEmailAsync(context.User.Identity?.Name!);
        if (user is null)
            return TypedResults.BadRequest("Nie można znaleźć użytkownika");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword!, request.NewPassword!);
        return result.Succeeded
            ? TypedResults.Ok()
            : TypedResults.BadRequest("Błąd przy zmianie hasła, sprawdź dane i wymagania.");
    }

    public static async Task<bool> CanLogIn(
        HttpContext context,
        LoginRequest? request,
        UserManager<AppUser> userManager,
        ILogger<AuthOperations> logger)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            logger.LogDebug("Empty request or empty email/password.");
            return false;
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        return user is { EmailConfirmed: true } && await userManager.CheckPasswordAsync(user, request.Password);
    }

    public static async Task<IResult> Register(
        HttpContext context,
        RegisterRequest request,
        UserManager<AppUser> userManager,
        IConfiguration config,
        ILogger<AuthOperations> logger)
    {
        var authSettings = config.GetSection("AuthSettings").Get<AuthSettings>() ?? new();

        var currentUser = context.User.Identity is null ? null : await userManager.FindByEmailAsync(context.User.Identity?.Name!);
        var isUserAdmin = currentUser is not null && await userManager.IsInRoleAsync(currentUser, AppRoles.Admin);

        if (!isUserAdmin && !authSettings.EnableRegisterModule)
            return TypedResults.BadRequest("Moduł rejestracji został wyłączony.");

        if (!IsValidRegisterRequest(request, out var error))
            return TypedResults.BadRequest(error);

        var newUser = new AppUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(newUser, request.Password);
        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Errors.First().Description);

        var createdUser = await userManager.FindByIdAsync(newUser.Id);
        if (createdUser is null)
            return TypedResults.NotFound("Can't find created user. Try again");

        await userManager.AddToRoleAsync(createdUser, AppRoles.User);
        if (createdUser.Email!.Equals(authSettings.MainAdminEmailAddress, StringComparison.OrdinalIgnoreCase))
            await userManager.AddToRoleAsync(createdUser, AppRoles.Admin);

        if (authSettings.AutoConfirmAccount)
        {
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
            await userManager.ConfirmEmailAsync(newUser, confirmationToken);
        }

        return TypedResults.Created($"/auth/Register", createdUser.Email);
    }

    public static async Task<IResult> Logout(
        HttpContext context,
        SignInManager<AppUser> signInManager,
        AppIdentityDbContext db,
        ILogger<AuthOperations> logger)
    {
        var currentUser = GetCurrentUserInfo(context);

        await signInManager.SignOutAsync();
        await CheckAndDeleteActiveUserTokens(db, currentUser.UserName, logger);

        logger.LogInformation("Successfully logged user {UserName} off", currentUser.UserName);
        return TypedResults.NoContent();
    }

    public static CurrentUser GetCurrentUserInfo(HttpContext context)
    {
        var roles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray();

        var claims = context.User.Claims
            .Where(c => c.Type != ClaimTypes.Role)
            .ToDictionary(c => c.Type, c => c.Value);

        claims[ClaimTypes.Role] = JsonSerializer.Serialize(roles);

        return new CurrentUser
        {
            IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false,
            UserName = context.User.Identity?.Name,
            Claims = claims
        };
    }

    public static async Task<IResult> ValidateToken(
        TokenValidationRequest request,
        ITokenValidationService tokenValidationService)
    {
        var isValid = tokenValidationService.IsValid(request.Token, request.Email);

        return isValid.Match<IResult>(
            _ => TypedResults.Ok(),
            _ => TypedResults.BadRequest()
        );
    }

    public static async Task<TokenModelDto> GetUserToken(
        HttpContext context,
        LoginRequest request,
        AppIdentityDbContext db,
        ITokenValidationService tokenValidationService,
        ILogger<AuthOperations> logger)
    {
        logger.LogInformation("Fetching Token for User {UserName} ...", request.Email);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return new();

        var token = db.UsersTokens.FirstOrDefault(t => t.Email == request.Email && t.ExpirationDate > DateTime.UtcNow);
        if (token == null)
            return new();

        var isValid = tokenValidationService.IsValid(token.Token, request.Email);
        return await isValid.Match<Task<TokenModelDto>>(
            _ => Task.FromResult(new TokenModelDto
            {
                Token = token.Token,
                Email = token.Email,
                ExpirationDate = token.ExpirationDate
            }),
            async _ =>
            {
                db.UsersTokens.Remove(token);
                await db.SaveChangesAsync();
                return new();
            });
    }

    public static async Task<TokenModelDto> RefreshToken(
        HttpContext context,
        LoginRequest request,
        UserManager<AppUser> userManager,
        AppIdentityDbContext db,
        ITokenValidationService tokenValidationService,
        IConfiguration config,
        ILogger<AuthOperations> logger)
    {
        var authSettings = config.GetSection("AuthSettings").Get<AuthSettings>() ?? new();

        logger.LogInformation("Refreshing token for {UserName} ...", request.Email);
        if (!await CanLogIn(context, request, userManager, logger))
            return new() { ExpirationDate = DateTime.UtcNow.AddMinutes(-1) };

        return CheckIfAnyActiveUserTokenExist(db, request.Email!, logger) ??
               await CreateNewUserToken(db, request.Email!, authSettings.DefaultTokenExpirationTimeInMinutes, logger);
    }

    // Helpers -----------------------

    private static bool IsValidRegisterRequest(RegisterRequest request, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            error = "Wszystkie pola są wymagane";
        else if (request.Password != request.PasswordConfirm)
            error = "Hasła się nie zgadzają!";
        return string.IsNullOrEmpty(error);
    }

    private static bool IsValidChangePasswordRequest(ChangePasswordRequest request, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
        {
            error = "Nieprawidłowe dane";
        }
        else if (request.NewPassword != request.ConfirmNewPassword)
        {
            error = "Hasła się nie zgadzają";
        }

        return string.IsNullOrEmpty(error);
    }

    private static async Task CheckAndDeleteActiveUserTokens(AppIdentityDbContext db, string? userName, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return;

        logger.LogDebug("Removing tokens from db for user {Email}...", userName);

        var tokens = db.UsersTokens.Where(t => t.Email == userName);
        if (tokens.Any())
        {
            db.UsersTokens.RemoveRange(tokens);
            await db.SaveChangesAsync();
        }
    }

    private static TokenModelDto? CheckIfAnyActiveUserTokenExist(AppIdentityDbContext db, string userEmail, ILogger logger)
    {
        logger.LogInformation("Searching DB for token for user {Email}...", userEmail);

        var token = db.UsersTokens.FirstOrDefault(ut => ut.Email == userEmail && ut.ExpirationDate > DateTime.UtcNow);

        if (token is null)
        {
            logger.LogDebug("Can't find any token. Returning null.");
            return null;
        }

        return new TokenModelDto
        {
            Email = token.Email,
            Token = token.Token,
            ExpirationDate = token.ExpirationDate
        };
    }

    private static async Task<TokenModelDto> CreateNewUserToken(AppIdentityDbContext db, string userEmail, int expirationTimeInMinutes, ILogger logger)
    {
        logger.LogDebug("Generating token for user {Email}...", userEmail);

        var random = new Random();
        var tokenValue = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 47)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        var token = new TokenModel
        {
            Email = userEmail,
            ExpirationDate = DateTime.UtcNow.AddMinutes(expirationTimeInMinutes),
            Token = tokenValue
        };

        db.UsersTokens.Add(token);
        await db.SaveChangesAsync();

        return new TokenModelDto
        {
            Token = token.Token,
            Email = token.Email,
            ExpirationDate = token.ExpirationDate
        };
    }
}