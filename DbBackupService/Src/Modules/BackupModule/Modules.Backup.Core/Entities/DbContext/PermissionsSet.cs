namespace Modules.Backup.Core.Entities.DbContext;

public sealed record PermissionsSet
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid ServerId { get; set; }
    public bool CanPerformBackup { get; set; } = true;
    public bool CanDownloadBackup { get; set; } = true;
    public bool CanTestBackup { get; set; } = true;
    public bool CanEditConn { get; set; } = true;
    public bool CanEditTunnels { get; set; } = true;
}