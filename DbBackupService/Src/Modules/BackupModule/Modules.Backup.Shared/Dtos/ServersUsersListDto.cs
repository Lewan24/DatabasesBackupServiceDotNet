using System.Security.Cryptography.X509Certificates;

namespace Modules.Backup.Shared.Dtos;

public record ServersUsersListDto(Guid ServerId, bool IsServerDisabled, string ServerConnectionName, int UsersWithAccess);