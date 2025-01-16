using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Modules.Backup.Api.EOApp;
using Modules.Backup.Api.EOBackup;
using Modules.Backup.Api.EOConfig;
using Modules.Backup.Api.EOTests;
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

    public static WebApplication MapRequiredEndpoints(this WebApplication app)
    {
        app.MapConfigEndpoints();
        app.MapAppEndpoints();
        app.MapBackupEndpoints();
        app.MapTestEndpoints();

        return app;
    }
}
