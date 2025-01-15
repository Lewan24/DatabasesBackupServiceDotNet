using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Modules.Backup.Application;
using Modules.Backup.Infrasctructure;

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
        ConfigEndpoints.MapConfigEndpoints(app);
        AppEndpoints.MapAppEndpoints(app);
        BackupEndpoints.MapBackupEndpoints(app);
        TestsEndpoints.MapTestEndpoints(app);

        return app;
    }
}
