using System.ComponentModel.DataAnnotations;

namespace Modules.Backup.Core.Models.DbContext;

public sealed record BackupSchedule
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public static DateTime FirstBackupDate { get; set; } = DateTime.Now;
    public DateTime NextBackupDate { get; set; }
    public int BackupIntervalInMinutes { get; set; } = 30;
    public Guid WordDaysId { get; set; }
    public Guid WorkHoursId { get; set; }
}

public sealed record BackupWorkDays
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public bool IsMondayWorking { get; set; } = true;
    public bool IsTuesdayWorking { get; set; } = true;
    public bool IsWednesdayWorking { get; set; } = true;
    public bool IsThursdayWorking { get; set; } = true;
    public bool IsFridayWorking { get; set; } = true;
    public bool IsSaturdayWorking { get; set; }
    public bool IsSundayWorking { get; set; }
}

public sealed record BackupWorkHours
{
    public Guid Id { get; set; }
    [Range(0, 24)]
    public byte StartHour { get; set; } = 0;
    [Range(0, 24)]
    public byte EndHour { get; set; } = 24;
}