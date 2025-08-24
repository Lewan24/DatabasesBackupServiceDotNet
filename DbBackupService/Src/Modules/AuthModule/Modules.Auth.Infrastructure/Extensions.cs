using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Modules.Auth.Infrastructure.DbContexts;
using Modules.Auth.Infrastructure.Repositories;

namespace Modules.Auth.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppIdentityDbContext>(opt =>
        {
            opt.UseSqlServer(connectionString);
            opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            opt.ConfigureWarnings(w => w.Ignore(RelationalEventId.MigrationsUserTransactionWarning));
        });

        services.AddTransient<IUserTokenService, UserTokenService>();

        return services;
    }
}