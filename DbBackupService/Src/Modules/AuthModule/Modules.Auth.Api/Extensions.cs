using Microsoft.Extensions.DependencyInjection;
using Modules.Auth.Application;
using Modules.Auth.Infrastructure;

namespace Modules.Auth.Api;

public static class Extensions
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, string connectionString)
    {
        services.AddInfrastructureLayer(connectionString);
        services.AddApplicationLayer();

        return services;
    }
}