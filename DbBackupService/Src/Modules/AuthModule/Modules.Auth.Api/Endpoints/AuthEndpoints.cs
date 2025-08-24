using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Auth.Api.Operations;
using Modules.Shared.Attributes;

namespace Modules.Auth.Api.Endpoints;

internal static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/auth")
            .RequireAuthorization();

        api.MapGet("Ping", AuthOperations.Ping)
            .WithSummary("Ping serwera autoryzacji")
            .AllowAnonymous();

        api.MapPost("Login", AuthOperations.Login)
            .WithSummary("Logowanie użytkownika")
            .AllowAnonymous();

        api.MapPost("ChangePassword", AuthOperations.ChangePassword)
            .WithSummary("Zmiana hasła użytkownika")
            .AddEndpointFilter<BasicTokenAuthorizationFilter>();

        api.MapPost("CanLogIn", AuthOperations.CanLogIn)
            .WithSummary("Sprawdzenie czy można zalogować użytkownika");

        api.MapPost("Register", AuthOperations.Register)
            .WithSummary("Rejestracja nowego użytkownika")
            .AllowAnonymous();

        api.MapPost("Logout", AuthOperations.Logout)
            .WithSummary("Wylogowanie użytkownika");

        api.MapGet("GetCurrentUser", AuthOperations.GetCurrentUserInfo)
            .WithSummary("Pobranie informacji o bieżącym użytkowniku")
            .AllowAnonymous();

        api.MapPost("ValidateToken", AuthOperations.ValidateToken)
            .WithSummary("Walidacja tokena")
            .AllowAnonymous()
            .WithMetadata(new AllowWithoutTokenValidationAttribute());

        api.MapPost("GetUserToken", AuthOperations.GetUserToken)
            .WithSummary("Pobranie tokena użytkownika");

        api.MapPost("RefreshToken", AuthOperations.RefreshToken)
            .WithSummary("Odświeżenie tokena użytkownika");

        return app;
    }
}
