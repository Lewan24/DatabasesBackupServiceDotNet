namespace Modules.Backup.Core.Entities.DbContext;

public sealed record BackupSchedule
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid DbConnectionId { get; set; }
    public static DateTime FirstBackupDate { get; set; } = DateTime.Now;
    public DateTime NextBackupDate { get; set; }
    public int BackupIntervalInMinutes { get; set; } = 30;
    public Guid WordDaysId { get; set; }
    public Guid WorkHoursId { get; set; }
}