using System.Configuration;
using Modules.Backup.Core.Entities.Databases;

namespace Modules.Backup.Core.Entities.DbContext;

public sealed record DbConnection
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DatabaseType DbType { get; set; } = DatabaseType.MySql;
    public string? DbName { get; set; } = string.Empty;
    public string? DbUser { get; set; } = string.Empty;
    public string? DbPasswd { get; set; } = string.Empty;
    [RegexStringValidator("[A-Za-z]+:[0-9]+")]
    public string? DbServerPort { get; set; } = string.Empty;
}