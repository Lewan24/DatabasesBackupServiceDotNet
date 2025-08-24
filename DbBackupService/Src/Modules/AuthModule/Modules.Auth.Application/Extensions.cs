using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Modules.Auth.Application.Services;
using Modules.Auth.Application.Services.Health;
using Modules.Auth.Application.Services.Token;
using Modules.Auth.Shared.Interfaces;
using Modules.Auth.Shared.Interfaces.Health;
using Modules.Auth.Shared.Interfaces.Token;

namespace Modules.Auth.Application;

public static class Extensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<ITokenValidationService, TokenValidationService>();
        
        return services;
    }

    public static IServiceCollection AddClientApplicationLayer(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddAuthorizationCore();

        services.AddSingleton<IAuthService, AuthService>();
        services.AddScoped<AuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<AuthStateProvider>());

        services.AddTransient<IAuthHealthService, AuthHealthService>();
        
        return services;
    }
}