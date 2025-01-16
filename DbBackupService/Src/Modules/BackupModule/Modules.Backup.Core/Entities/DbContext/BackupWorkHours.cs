using System.ComponentModel.DataAnnotations;

namespace Modules.Backup.Core.Entities.DbContext;

public sealed record BackupWorkHours
{
    public Guid Id { get; set; }
    [Range(0, 24)]
    public byte StartHour { get; set; } = 0;
    [Range(0, 24)]
    public byte EndHour { get; set; } = 24;
}