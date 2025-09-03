using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Modules.Backup.Api.Backups;
using Modules.Backup.Application;
using Modules.Backup.Infrastructure;

namespace Modules.Backup.Api;

public static class Extensions
{
    public static IServiceCollection AddBackupModule(this IServiceCollection services)
    {
        services.AddInfrastructureLayer();
        services.AddApplicationLayer();

        return services;
    }

    public static WebApplication MapBackupModuleEndpoints(this WebApplication app)
    {
        app.MapBackupEndpoints();

        return app;
    }
}