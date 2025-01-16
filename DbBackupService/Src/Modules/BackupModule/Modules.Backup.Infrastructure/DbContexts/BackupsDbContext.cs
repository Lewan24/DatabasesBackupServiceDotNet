using Microsoft.EntityFrameworkCore;
using Modules.Backup.Core.Entities.DbContext;

namespace Modules.Backup.Infrastructure.DbContexts;

public sealed class BackupsDbContext : DbContext
{
    public DbSet<BackupSchedule> Schedules { get; set; }
    public DbSet<BackupWorkDays> WorkDays { get; set; }
    public DbSet<BackupWorkHours> WorkHours { get; set; }
    public DbSet<DbConnection> DbConnections { get; set; }
    
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
