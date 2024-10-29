namespace Core.Entities.Models;

public record DatabaseConfigModel
{
    public DatabaseType DbType { get; init; } = DatabaseType.MySql;
    public string? DbName { get; init; }
    public string? DbUser { get; init; }
    public string? DbPasswd { get; init; }
    public string? DbServerAndPort { get; init; }
}

public enum DatabaseType
{
    MySql,
    PostgreSql
}