using Microsoft.EntityFrameworkCore;
using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Infrastructure.DbContexts;

public sealed class BackupsDbContext : DbContext
{
    // TODO: Check all properties and available FK
    public DbSet<DbServerConnection> DbConnections { get; set; }
    public DbSet<DbServerTunnel> DbServerTunnels { get; set; }
    public DbSet<BackupSchedule> Schedules { get; set; }
    public DbSet<PerformedBackup> Backups { get; set; }
    public DbSet<BackupTest> BackupsTests { get; set; }
    public DbSet<AutomaticBackupTestConfig> AutomaticBackupTestConfigs { get; set; }
    public DbSet<PermissionsSet> PermissionsSets { get; set; }
    public DbSet<UsersPermissionsSets> UsersPermissions { get; set; }
    public DbSet<UserNotificationsSettings> UsersNotificationsSettings { get; set; }
    
    private string DbPath { get; }

    public BackupsDbContext()
    {
        var dbFolder = Path.Combine("/app", "db");

        if (!Directory.Exists(dbFolder))
            Directory.CreateDirectory(dbFolder);

        DbPath = Path.Join(dbFolder, "BackupsConfiguration.db");

        Database.Migrate();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
