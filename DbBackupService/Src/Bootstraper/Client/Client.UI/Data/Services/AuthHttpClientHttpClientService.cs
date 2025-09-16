using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Client.UI.Data.Interfaces;
using Modules.Auth.Shared.ActionsRequests;
using Modules.Auth.Shared.Entities;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;

namespace Client.UI.Data.Services;

internal class AuthHttpClientHttpClientService(
    HttpClient httpClient,
    IServiceScopeFactory scopeFactory,
    ILogger<AuthHttpClientHttpClientService> logger)
    : IAuthHttpClientService
{
    private const string StorageTokenKeyName = "TokenKey";
    public TokenModelDto? UserToken { get; set; } = new();

    public async Task<(bool Success, string? Msg)> Login(LoginRequest? loginRequest)
    {
        try
        {
            var loginResult = await httpClient.PostAsJsonAsync("api/Auth/Login", loginRequest);

            if (loginResult.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
            {
                var errorMsg = await loginResult.Content.ReadAsStringAsync();
                logger.LogWarning("Login failed: {Msg}", errorMsg);
                return (false, errorMsg);
            }

            var retrieveUserTokenResult = await httpClient.PostAsJsonAsync("api/Auth/GetUserToken", loginRequest);
            UserToken = JsonConvert.DeserializeObject<TokenModelDto>(
                await retrieveUserTokenResult.Content.ReadAsStringAsync());

            if (loginRequest?.RememberMe ?? false)
                await SaveTokenToStorage();
            else
                await RemoveTokenFromStorage();

            logger.LogInformation("Login succeeded for user: {Email}", loginRequest?.Email);
            return (true, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unexpected error during login");
            return (false, "Unexpected error during login");
        }
    }

    public async Task<bool> TryLogin(LoginRequest? request)
    {
        try
        {
            var result = await httpClient.PostAsJsonAsync("api/Auth/CanLogIn", request);
            result.EnsureSuccessStatusCode();

            var canLogin = JsonConvert.DeserializeObject<bool>(await result.Content.ReadAsStringAsync());
            logger.LogInformation("TryLogin for user {Email}: {Result}", request?.Email, canLogin);
            return canLogin;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "TryLogin failed");
            return false;
        }
    }

    public async Task<((bool Success, string? Msg) Validation, string? UserName)> Register(
        RegisterRequest? registerRequest)
    {
        try
        {
            var result = await httpClient.PostAsJsonAsync("api/Auth/Register", registerRequest);

            if (result.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
            {
                var msg = await result.Content.ReadAsStringAsync();
                logger.LogWarning("Register failed: {Msg}", msg);
                return ((false, msg), null);
            }

            var username = await result.Content.ReadAsStringAsync();
            logger.LogInformation("Register succeeded for user: {Email}", registerRequest?.Email);
            return ((true, null), username);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unexpected error during register");
            return ((false, "Unexpected error during register"), null);
        }
    }

    public async Task Logout()
    {
        try
        {
            await RemoveTokenFromStorage();
            await httpClient.PostAsync("api/Auth/Logout", null);
            logger.LogInformation("User logged out");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Logout failed");
        }
    }

    public async Task<CurrentUser?> GetCurrentUserInfo()
    {
        try
        {
            await CheckRetrieveSaveStorageToken();
            var user = await httpClient.GetFromJsonAsync<CurrentUser>("api/Auth/GetCurrentUser");
            logger.LogInformation("Fetched current user info");
            return user;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "GetCurrentUserInfo failed");
            return null;
        }
    }

    public async Task RefreshToken(LoginRequest request)
    {
        try
        {
            var result = await httpClient.PostAsJsonAsync("api/Auth/RefreshToken", request);

            UserToken = JsonConvert.DeserializeObject<TokenModelDto>(await result.Content.ReadAsStringAsync());

            if (request.RememberMe)
                await SaveTokenToStorage();
            else
                await RemoveTokenFromStorage();

            logger.LogInformation("Token refreshed for user: {Email}", request.Email);
        }
        catch (Exception ex)
        {
            logger.LogInformation("Error while refreshing Token. Error: {Error}", ex.Message);
        }
    }

    public async Task<OneOf<True, False>> IsValid()
    {
        if (string.IsNullOrWhiteSpace(UserToken?.Token) || string.IsNullOrWhiteSpace(UserToken.Email))
            return new False();

        try
        {
            var tokenValidationRequest = new TokenValidationRequest(UserToken.Token, UserToken.Email);
            var result = await httpClient.PostAsJsonAsync("api/Auth/ValidateToken", tokenValidationRequest);

            if (result.StatusCode != HttpStatusCode.OK)
                return new False();

            return new True();
        }
        catch (Exception ex)
        {
            logger.LogInformation("Error while validating Token. Error: {Error}", ex.Message);
            return new False();
        }
    }

    private async Task CheckRetrieveSaveStorageToken()
    {
        UserToken = await GetLocalStorageService().GetItemAsync<TokenModelDto?>(StorageTokenKeyName) ??
                    new TokenModelDto();
        logger.LogDebug("Token retrieved from storage: {HasToken}", UserToken?.Token != null);
    }

    private async Task SaveTokenToStorage()
    {
        await GetLocalStorageService().SetItemAsync(StorageTokenKeyName, UserToken);
        logger.LogDebug("Token saved to storage");
    }

    private async Task RemoveTokenFromStorage()
    {
        await GetLocalStorageService().RemoveItemAsync(StorageTokenKeyName);
        logger.LogDebug("Token removed from storage");
    }

    private ILocalStorageService GetLocalStorageService()
    {
        using var scope = scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ILocalStorageService>();
    }
}