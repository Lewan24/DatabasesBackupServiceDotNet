namespace Modules.Backup.Core.Entities.DbContext;

public sealed record UsersPermissionsSets
{
    public Guid UserId { get; set; }
    public Guid PermissionSetId { get; set; }
}