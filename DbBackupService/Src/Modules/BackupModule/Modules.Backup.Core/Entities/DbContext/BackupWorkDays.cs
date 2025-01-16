namespace Modules.Backup.Core.Entities.DbContext;

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