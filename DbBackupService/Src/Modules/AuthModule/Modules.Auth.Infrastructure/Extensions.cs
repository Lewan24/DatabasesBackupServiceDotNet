using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Modules.Auth.Infrastructure.DbContexts;
using Modules.Auth.Infrastructure.Repositories;

namespace Modules.Auth.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
    {
        services.AddDbContext<AppIdentityDbContext>();
        services.AddTransient<IUserTokenService, UserTokenService>();

        return services;
    }
}