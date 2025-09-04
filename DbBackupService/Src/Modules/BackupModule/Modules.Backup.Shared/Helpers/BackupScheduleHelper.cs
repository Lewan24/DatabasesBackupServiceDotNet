using Modules.Backup.Shared.Dtos;

namespace Modules.Backup.Shared.Helpers;

public static class BackupScheduleHelper
{
    public static DateTime GetNextDateTime(DayOfWeek day, TimeOnly time, DateTime now)
    {
        var daysUntil = ((int)day - (int)now.DayOfWeek + 7) % 7;
        var date = now.Date.AddDays(daysUntil).Add(time.ToTimeSpan());
        if (date <= now)
            date = date.AddDays(7);
        return date;
    }
}