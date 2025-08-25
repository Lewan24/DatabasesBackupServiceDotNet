using System.Security.Claims;
using System.Text.Json;
using Client.UI.Data.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Modules.Auth.Shared.ActionsRequests;
using Modules.Auth.Shared.Entities;

namespace Client.UI.Data.Services;

public sealed class AuthStateProvider(IAuthHttpClientService api, NavigationManager nav, ILogger<AuthStateProvider> logger)
    : AuthenticationStateProvider
{
    private readonly ILoggingIn _loginApi = api;
    private readonly IRegistration _registerApi = api;
    private readonly IUserToken _tokenApi = api;
    private readonly ICurrentUserInfo _userInfoApi = api;

    private CurrentUser? _currentUser;

    private async Task<CurrentUser?> CurrentUserInfo()
    {
        if (_currentUser is not null && _currentUser.IsAuthenticated)
            return _currentUser;

        _currentUser = await _userInfoApi.GetCurrentUserInfo();

        return _currentUser;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        try
        {
            var userInfo = await CurrentUserInfo();
            if (userInfo!.IsAuthenticated)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, _currentUser!.UserName!) }
                    .Concat(_currentUser.Claims!.Select(c => new Claim(c.Key, c.Value))).ToList();
                var roles = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                claims.Remove(roles!);
                var rolesString = JsonSerializer.Deserialize<string[]>(roles!.Value);

                if (rolesString != null)
                    foreach (var role in rolesString)
                        claims.Add(new Claim(ClaimTypes.Role, role));

                identity = new ClaimsIdentity(claims, "Server authentication");
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning("Request failed. Error: {Error}", ex.Message);
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<bool> IsUserAuthenticated()
    {
        if (_currentUser is not null)
            return _currentUser.IsAuthenticated;

        return (await CurrentUserInfo())!.IsAuthenticated;
    }
    
    public async Task<(bool Success, string? Msg)> Login(LoginRequest? loginRequest)
    {
        var loginResult = await _loginApi.Login(loginRequest);

        if (loginResult.Success)
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        return loginResult;
    }

    public async Task<bool> TryLogin(string? password)
    {
        try
        {
            var result = await _loginApi.TryLogin(new LoginRequest
                { Password = password!, Email = _currentUser?.UserName! });
            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Error thrown during test login: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    public async Task<(bool Success, string? Msg)> Register(RegisterRequest? registerRequest)
    {
        var registerResult = await _registerApi.Register(registerRequest);

        if (!registerResult.Validation.Success)
            return (false, registerResult.Validation.Msg);

        return (true, registerResult.UserName);
    }

    public async Task Logout()
    {
        try
        {
            await _loginApi.Logout();

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception)
        {
            nav.NavigateTo("/");
        }
    }

    /// <summary>
    ///     Get current user token, if the token is empty or invalid then auth service redirects user to ConfirmPassword page
    /// </summary>
    /// <param name="userPassword">Password typed by user</param>
    /// <param name="returnPageAfterValidPasswordAuthorization">
    ///     Page scheme (every '/' must be replaced with '-') like:
    ///     module-page-subpage-[...]
    /// </param>
    /// <param name="rememberMe"></param>
    /// <returns>Token string</returns>
    public async Task<string?> AuthorizeUser(string? userPassword = "",
        string? returnPageAfterValidPasswordAuthorization = "", bool rememberMe = false)
    {
        return await GetCurrentUserToken(userPassword, returnPageAfterValidPasswordAuthorization, rememberMe);
    }

    private async Task<string?> GetCurrentUserToken(string? password = "", string? pageName = "",
        bool rememberMe = false)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            if ((await _tokenApi.IsValid()).IsT1)
                nav.NavigateTo($"/account/ConfirmPassword/{pageName}");

            return _tokenApi.UserToken!.Token;
        }

        try
        {
            var currentUser = await CurrentUserInfo();
            await _tokenApi.RefreshToken(new LoginRequest
                { Password = password, Email = currentUser?.UserName!, RememberMe = rememberMe });
        }
        catch (Exception ex)
        {
            logger.LogInformation("Wystąpił błąd podczas odświeżania Tokenu Uwierzytelniającego. Błąd: {Error}",
                ex.Message);
        }

        return _tokenApi.UserToken?.Token;
    }
}