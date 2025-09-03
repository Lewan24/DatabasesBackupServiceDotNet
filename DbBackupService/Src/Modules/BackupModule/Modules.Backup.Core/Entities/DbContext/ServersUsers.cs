namespace Modules.Backup.Core.Entities.DbContext;

public sealed record ServersUsers
{
    public Guid UserId { get; set; }
    public Guid ServerId { get; set; }
}