using OneOf;
using OneOf.Types;

namespace Modules.Backup.Application.Interfaces;

public interface IDbBackupService
{
    Task<OneOf<Success, string>> BackupFromSchedules();
    Task<OneOf<Success, string>> BackupDb(Guid serverId);
}