using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Modules.Auth.Core.Entities;
using Modules.Auth.Shared.Static.Entities;
using Modules.Shared.Common;

namespace Modules.Auth.Infrastructure.DbContexts;

public sealed class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<TokenModel> UsersTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        DbCommon.CreateDbDirectoryIfNotExists();
        optionsBuilder.UseSqlite($"Data Source={DbCommon.DbPath}");
        
        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = Guid.CreateVersion7().ToString(), Name = AppRoles.User,
                NormalizedName = AppRoles.User.ToUpper()
            },
            new IdentityRole
            {
                Id = Guid.CreateVersion7().ToString(), Name = AppRoles.Admin,
                NormalizedName = AppRoles.Admin.ToUpper()
            });

        base.OnModelCreating(builder);
    }
}