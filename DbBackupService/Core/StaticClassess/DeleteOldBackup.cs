using System.Globalization;
using System.Text.RegularExpressions;
using NLog;

namespace Core.StaticClassess;

public static class DeleteOldBackup
{
    public static Task Delete(string directoryPath, int daysThreshold, Logger logger)
    {
        try
        {
            var zipFiles = Directory.GetFiles(directoryPath, "*.zip");

            var currentDate = DateTime.Now;

            foreach (var zipFile in zipFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(zipFile);
                var match = Regex.Match(fileName, @"\d{2}\.\d{2}\.\d{2}_\d{2}\.\d{2}");

                if (!match.Success) continue;
                if (!DateTime.TryParseExact(match.Value, "dd.MM.yy_HH.mm", null, DateTimeStyles.None,
                        out var backupDateTime)) continue;
                var difference = currentDate - backupDateTime;

                if (!(difference.TotalDays > daysThreshold)) continue;

                File.Delete(zipFile);
                logger.Info($"Deleted old backup: {Path.GetFileName(zipFile)}");
            }
            
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            logger.Warn(e, "Error thrown while deleting old backups");
            return Task.CompletedTask;
        }
    }
}