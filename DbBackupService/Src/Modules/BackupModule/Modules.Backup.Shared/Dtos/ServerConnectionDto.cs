using Modules.Backup.Shared.Enums;

namespace Modules.Backup.Shared.Dtos;

public record ServerConnectionDto
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string ConnectionName { get; set; } = "Example";
    public required string ServerHost { get; set; }
    public required short ServerPort { get; set; }
    public required string DbName { get; set; }
    public DatabaseType DbType { get; set; } = DatabaseType.MySql;
    public required string DbUser { get; set; }
    public string? DbPasswd { get; set; }
    public bool IsTunnelRequired { get; set; }
    public ServerTunnelDto? Tunnel { get; set; }
    //public Guid AutoTestBackupsConfigId { get; set; }
}