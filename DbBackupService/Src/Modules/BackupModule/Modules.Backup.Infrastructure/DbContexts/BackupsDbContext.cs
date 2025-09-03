using Microsoft.EntityFrameworkCore;
using Modules.Backup.Core.Entities.DbContext;
using Modules.Shared.Common;

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
    public DbSet<ServersUsers> UsersServers { get; set; }
    public DbSet<UserNotificationsSettings> UsersNotificationsSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbCommon.DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServersUsers>().HasNoKey();
    }
}