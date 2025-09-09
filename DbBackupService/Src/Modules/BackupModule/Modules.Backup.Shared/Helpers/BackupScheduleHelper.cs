using Modules.Backup.Shared.Dtos;

namespace Modules.Backup.Shared.Helpers;

public static class BackupScheduleHelper
{
    private static DateTime GetNextDateTime(DayOfWeek day, TimeOnly time, DateTime now)
    {
        var daysUntil = ((int)day - (int)now.DayOfWeek + 7) % 7;
        var date = now.Date.AddDays(daysUntil).Add(time.ToTimeSpan());
        if (date <= now)
            date = date.AddDays(7);
        return date;
    }

    public static DateTime GetNextDateTime(BackupsScheduleDto schedule)
    {
        if (!schedule.SelectedDays.Any() || !schedule.SelectedTimes.Any())
            return DateTime.Now.AddHours(1);
        
        var candidates = schedule.SelectedDays
            .SelectMany(d => schedule.SelectedTimes, (day, time) => GetNextDateTime(day, time, DateTime.Now))
            .ToList();

        return candidates.Min();
    }
    
    public static string GetNextBackupDateString(BackupsScheduleDto schedule)
    {
        if (!schedule.SelectedDays.Any() || !schedule.SelectedTimes.Any())
            return "-";

        var now = DateTime.Now;
        var candidates = schedule.SelectedDays
            .SelectMany(d => schedule.SelectedTimes, (day, time) => GetNextDateTime(day, time, now))
            .ToList();

        return candidates.Min().ToString("g");
    }
}