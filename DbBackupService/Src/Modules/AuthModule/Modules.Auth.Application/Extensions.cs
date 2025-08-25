using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Modules.Auth.Application.Services;
using Modules.Auth.Shared.Interfaces;

namespace Modules.Auth.Application;

public static class Extensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<ITokenValidationService, TokenValidationService>();
        
        return services;
    }
}