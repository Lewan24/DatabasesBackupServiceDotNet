using Modules.Backup.Core.Entities.Models;

namespace Modules.Backup.Core.Entities.DbContext;

public sealed record BackupSchedule
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public bool IsEnabled { get; set; } = true;
    public Guid DbConnectionId { get; set; }

    /// <summary>
    /// <see cref="BackupScheduleConfiguration"/>
    /// </summary>
    public required string ConfigurationJson { get; set; }
    
    public DateTime NextBackupDate { get; set; }
}