namespace Modules.Backup.Core.Entities.Models;

public record BackupScheduleConfiguration
{
    public BackupDayTime Monday { get; set; } = new(EWeekDay.Monday);
    public BackupDayTime Tuesday { get; set; } = new(EWeekDay.Tuesday);
    public BackupDayTime Wednesday { get; set; } = new(EWeekDay.Wednesday);
    public BackupDayTime Thursday { get; set; } = new(EWeekDay.Thursday);
    public BackupDayTime Friday { get; set; } = new(EWeekDay.Friday);
    public BackupDayTime Saturday { get; set; } = new(EWeekDay.Saturday);
    public BackupDayTime Sunday { get; set; } = new(EWeekDay.Sunday);
}