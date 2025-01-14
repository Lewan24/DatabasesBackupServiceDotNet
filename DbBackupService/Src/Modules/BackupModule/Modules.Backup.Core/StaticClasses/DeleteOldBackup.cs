using System.Text.RegularExpressions;
using NLog;

namespace Modules.Backup.Core.StaticClasses;

public static partial class DeleteOldBackup
{
    public static Task Delete(string directoryPath, int daysThreshold, Logger logger)
    {
        try
        {
            var zipFiles = Directory.GetFiles(directoryPath, "*.zip");

            foreach (var zipFile in zipFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(zipFile);
                var match = Regex.Match(fileName, @"\d{1,2}\.\d{1,2}\.\d{2,4}");

                if (!match.Success) continue;
                var backupDateTime = DateTime.Parse(match.Value);

                var difference = DateTime.Now - backupDateTime;

                if (difference.TotalDays < daysThreshold)
                    continue;

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