using Microsoft.EntityFrameworkCore;
using Modules.Backup.Core.Models.DbContext;

namespace Modules.Backup.Infrasctructure.DbContexts;

public class DbBackupScheduleContext : DbContext
{
    public DbSet<BackupSchedule> Schedules { get; set; }
    public DbSet<BackupWorkDays> WorkDays { get; set; }
    public DbSet<BackupWorkHours> WorkHours { get; set; }

    private string DbPath { get; }

    public DbBackupScheduleContext()
    {
        var DbFolder = Path.Combine("/app", "db");

        if (!Directory.Exists(DbFolder))
            Directory.CreateDirectory(DbFolder);

        DbPath = Path.Join(DbFolder, "BackupsSchedule.db");

        Database.Migrate();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
