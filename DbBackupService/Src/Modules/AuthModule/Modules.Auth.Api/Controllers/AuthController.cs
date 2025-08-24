using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Auth.Api.Entities;
using Modules.Auth.Core.Entities;
using Modules.Auth.Infrastructure.DbContexts;
using Modules.Auth.Shared.ActionsRequests;
using Modules.Auth.Shared.Entities.Tokens;
using Modules.Auth.Shared.Interfaces.Token;
using Modules.Auth.Shared.Static.Entities;
using Modules.Shared.Attributes;
using Modules.Shared.Common;

namespace Modules.Auth.Api.Controllers;

[Authorize]
public class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AppIdentityDbContext context,
    ITokenValidationService tokenValidationService,
    IConfiguration config,
    ILogger<AuthController> logger)
    : ApiBaseController
{
    private readonly AuthSettings _authSettings = config.GetSection("AuthSettings").Get<AuthSettings>() ?? new();

    [HttpGet]
    [Route("Ping")]
    public IActionResult Ping()
    {
        logger.LogInformation("Ping from {UserEmail}", User.Identity?.Name);
        return Ok("pong");
    }
    
    [HttpPost("Login")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest? request)
    {
        logger.LogInformation("Trying to log {UserName} in...", request?.Email);
        if (!await CanLogIn(request))
        {
            logger.LogDebug("Invalid login attempt.");
            return BadRequest("Nie można zalogować. Sprawdź poprawność danych.");
        }

        var user = await userManager.FindByEmailAsync(request!.Email!);
        if (user is null)
        {
            logger.LogDebug("Can't find user in DB");
            return NotFound("Taki użytkownik nie istnieje.");
        }

        if (!user.EmailConfirmed)
        {
            logger.LogInformation("User {UserName} is not confirmed.", user.Email);
            return BadRequest("Konto zablokowane.");
        }

        await signInManager.SignInAsync(user, request.RememberMe);

        logger.LogInformation("User {UserName} successfully logged in. Generating token...", user.Email);
        var result = await GetUserToken(request);
        if (string.IsNullOrWhiteSpace(result.Token))
            await CreateNewUserToken(request.Email!, _authSettings.DefaultTokenExpirationTimeInMinutes);

        return Accepted();
    }

    [HttpPost("ChangePassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [BasicTokenAuthorization]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        logger.LogInformation("Changing password for {UserName}...", User.Identity?.Name);
        if (!IsValidChangePasswordRequest(request, out var error))
            return BadRequest(error);

        var user = await userManager.FindByEmailAsync(User.Identity?.Name!);
        if (user is null)
            return BadRequest("Nie można znaleźć użytkownika");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword!, request.NewPassword!);
        return result.Succeeded ? Ok() : BadRequest("Błąd przy zmianie hasłą, sprawdź czy dane się zgadzają i czy hasło spełnia wymagania: Duża i mała litera, cyfra, znak specjalny.");
    }

    [HttpPost("CanLogIn")]
    [Produces(typeof(bool))]
    public async Task<bool> CanLogIn(LoginRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            logger.LogDebug("Empty request or empty email/password.");
            return false;
        }
        
        var user = await userManager.FindByEmailAsync(request.Email);
        return user is { EmailConfirmed: true } && await userManager.CheckPasswordAsync(user, request.Password);
    }

    [HttpPost("Register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = User.Identity is null ? null : await userManager.FindByEmailAsync(User.Identity?.Name!);
        
        var isUserAdmin = false;
        
        if (user is not null)
            isUserAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);
        
        if (!isUserAdmin && !_authSettings.EnableRegisterModule)
            return BadRequest("Moduł rejestracji został wyłączony przez Administratora.");

        if (!IsValidRegisterRequest(request, out var error))
            return BadRequest(error);

        var newUser = new AppUser { UserName = request.Email, Email = request.Email };

        var result = await userManager.CreateAsync(newUser, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.First().Description);

        var createdUser = await userManager.FindByIdAsync(newUser.Id);
        if (createdUser is null)
            return NotFound("Can't find created user. Try again");

        await userManager.AddToRoleAsync(createdUser, AppRoles.User);
        if (createdUser.Email!.Equals(_authSettings.MainAdminEmailAddress, StringComparison.OrdinalIgnoreCase))
            await userManager.AddToRoleAsync(createdUser, AppRoles.Admin);

        if (_authSettings.AutoConfirmAccount)
        {
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
            await userManager.ConfirmEmailAsync(newUser, confirmationToken);
        }

        return CreatedAtAction(nameof(Register), createdUser.Email);
    }

    [HttpPost("Logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var currentUser = GetCurrentUserInfo();

        await signInManager.SignOutAsync();
        await CheckAndDeleteActiveUserTokens(currentUser.UserName);

        logger.LogInformation("Successfully logged user {UserName} off", currentUser.UserName);
        return NoContent();
    }

    [HttpGet("GetCurrentUser")]
    [Produces(typeof(CurrentUser))]
    [AllowAnonymous]
    public CurrentUser GetCurrentUserInfo()
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray();

        var claims = User.Claims
            .Where(c => c.Type != ClaimTypes.Role)
            .ToDictionary(c => c.Type, c => c.Value);

        claims[ClaimTypes.Role] = JsonSerializer.Serialize(roles);

        return new CurrentUser
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserName = User.Identity?.Name,
            Claims = claims
        };
    }

    [HttpPost("ValidateToken")]
    [AllowAnonymous]
    [AllowWithoutTokenValidation]
    public async Task<IActionResult> ValidateToken(TokenValidationRequest request)
    {
        var isValid = tokenValidationService.IsValid(request.Token, request.Email);

        return isValid.Match<IActionResult>(
            _ => Ok(),
            _ => BadRequest()
        );
    }
    
    [HttpPost("GetUserToken")]
    [Produces(typeof(TokenModelDto))]
    public async Task<TokenModelDto> GetUserToken(LoginRequest request)
    {
        logger.LogInformation("Fetching Token for User {UserName} ...", request.Email);
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return new();

        var token = context.UsersTokens.FirstOrDefault(t => t.Email == request.Email && t.ExpirationDate > DateTime.UtcNow);
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
                context.UsersTokens.Remove(token);
                await context.SaveChangesAsync();
                return new();
            });
    }

    [HttpPost("RefreshToken")]
    [Produces(typeof(TokenModelDto))]
    public async Task<TokenModelDto> RefreshToken(LoginRequest request)
    {
        logger.LogInformation("Refreshing token for {UserName} ...", request.Email);
        if (!await CanLogIn(request))
        {
            logger.LogDebug("Returning expired token.");
            return new() { ExpirationDate = DateTime.UtcNow.AddMinutes(-1) };
        }

        return CheckIfAnyActiveUserTokenExist(request.Email!) ??
               await CreateNewUserToken(request.Email!, _authSettings.DefaultTokenExpirationTimeInMinutes);
    }

    private bool IsValidRegisterRequest(RegisterRequest request, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            error = "Wszystkie pola są wymagane";
        else if (request.Password != request.PasswordConfirm)
            error = "Hasła się nie zgadzają!";
        return string.IsNullOrEmpty(error);
    }

    private bool IsValidChangePasswordRequest(ChangePasswordRequest request, out string error)
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

    private async Task CheckAndDeleteActiveUserTokens(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return;

        logger.LogDebug("Removing tokens from db for user {Email}...", userName);
        
        var tokens = context.UsersTokens.Where(t => t.Email == userName);
        if (tokens.Any())
        {
            context.UsersTokens.RemoveRange(tokens);
            await context.SaveChangesAsync();
        }
    }

    private TokenModelDto? CheckIfAnyActiveUserTokenExist(string userEmail)
    {
        logger.LogInformation("Searching DB for token for user {Email}...", userEmail);
        
        var token = context.UsersTokens
            .FirstOrDefault(ut => ut.Email == userEmail && ut.ExpirationDate > DateTime.UtcNow);

        if (token is null)
        {
            logger.LogDebug("Can't find any token. Returning null.");
            return null;
        }
        
        logger.LogDebug("Returning found token {Token} for user {Email}", token.Token, token.Email);
        return new TokenModelDto
        {
            Email = token.Email,
            Token = token.Token,
            ExpirationDate = token.ExpirationDate
        };
    }

    private async Task<TokenModelDto> CreateNewUserToken(string userEmail, int expirationTimeInMinutes)
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

        logger.LogDebug("Saving token ({Token}) with expiration date ({TokenExpirationDate}) in DB...", token.Token, token.ExpirationDate.ToString("dd/MM/yyyy - HH:mm:ss"));
        var result = context.UsersTokens.Add(token);
        await context.SaveChangesAsync();

        logger.LogDebug("Returning created token...");
        return new TokenModelDto
        {
            Token = result.Entity.Token,
            Email = result.Entity.Email,
            ExpirationDate = result.Entity.ExpirationDate
        };
    }
}
