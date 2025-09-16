namespace Modules.Backup.Core.Entities.DbContext;

public sealed record BackupTest
{
    public Guid Id { get; set; }
    public Guid BackupId { get; set; }
    public DateTime TestedOn { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}