namespace Modules.Backup.Core.Entities.DbContext;

public sealed record AutomaticBackupTestConfig
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool ShouldTestEveryBackup { get; set; } = true;
    public short TestFrequency { get; set; }
}