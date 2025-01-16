using Microsoft.Extensions.DependencyInjection;
using Modules.Backup.Infrastructure.Interfaces;
using Modules.Backup.Infrastructure.Services;

namespace Modules.Backup.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
    {
        services.AddScoped<IDbScheduleRepo, DbScheduleRepo>();
        
        return services;
    }
}