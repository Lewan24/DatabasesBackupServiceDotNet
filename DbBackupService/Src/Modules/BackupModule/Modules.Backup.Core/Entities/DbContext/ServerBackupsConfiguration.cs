using System.ComponentModel.DataAnnotations.Schema;

namespace Modules.Backup.Core.Entities.DbContext;

public sealed class ServerBackupsConfiguration
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ServerId { get; set; }
    public int TimeInDaysToHoldBackups { get; set; } = 4;
    [NotMapped]
    public string BackupSaveDirectory { get; set; } = "/backups";

    public (string Path, string FileName) GetBackupPathAndBackupFileName(DbServerConnection server, string? fileName)
    {
        var date = $"{DateTime.Now:yyyyMMddHHmm}";

        if (!string.IsNullOrWhiteSpace(fileName))
            date = fileName;
            
        var path =
            $"{BackupSaveDirectory}/{server.ServerHost}:{server.ServerPort}/:{server.DbName}/{date}";

        return (path, date);
    }
}