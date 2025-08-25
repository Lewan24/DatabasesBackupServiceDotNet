using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Auth.Application.Interfaces;
using Modules.Auth.Shared.ActionsRequests;
using Modules.Auth.Shared.Entities;

namespace Modules.Auth.Api.Operations;

internal abstract record AuthOperations
{
    public static IResult Ping(HttpContext context, ILogger<AuthOperations> logger)
    {
        logger.LogInformation("Ping from {UserEmail}", context.User.Identity?.Name);
        return TypedResults.Ok("pong");
    }

    public static Task<IResult> Login(
        HttpContext context,
        LoginRequest? request,
        [FromServices] IAuthService authService)
        => authService.Login(context, request);

    public static Task<IResult> ChangePassword(
        HttpContext context,
        ChangePasswordRequest request,
        [FromServices] IAuthService authService)
        => authService.ChangePassword(context, request);

    public static Task<bool> CanLogIn(
        HttpContext context,
        LoginRequest? request,
        [FromServices] IAuthService authService)
        => authService.CanLogIn(context, request);

    public static Task<IResult> Register(
        HttpContext context,
        RegisterRequest request,
        [FromServices] IAuthService authService)
        => authService.Register(context, request);

    public static Task<IResult> Logout(
        HttpContext context,
        [FromServices] IAuthService authService)
        => authService.Logout(context);

    public static CurrentUser GetCurrentUserInfo(
        HttpContext context,
        [FromServices] IAuthService authService)
        => authService.GetCurrentUserInfo(context);

    public static Task<IResult> ValidateToken(
        TokenValidationRequest request,
        [FromServices] IAuthService authService)
        => authService.ValidateToken(request);

    public static Task<TokenModelDto> GetUserToken(
        HttpContext context,
        LoginRequest request,
        [FromServices] IAuthService authService)
        => authService.GetUserToken(context, request);

    public static Task<TokenModelDto> RefreshToken(
        HttpContext context,
        LoginRequest request,
        [FromServices] IAuthService authService)
        => authService.RefreshToken(context, request);
}