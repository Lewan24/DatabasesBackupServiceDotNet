using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Modules.Backup.Application;

namespace Modules.Backup.Api;

public static class Extensions
{
    public static IServiceCollection AddBackupModule(this IServiceCollection services)
    {
        services.AddApplicationLayer();

        return services;
    }

    public static WebApplication MapBackupEndpoints(this WebApplication app)
    {
        Endpoints.MapConfigEndpoints(app);
        Endpoints.MapBackupEndpoints(app);
        Endpoints.MapTestEndpoints(app);

        return app;
    }
}
