namespace Core.Entities.Models;

public class DatabaseConfigModel
{
    public DatabaseType DbType { get; set; } = DatabaseType.MySql;
    public string? DbName { get; set; }
    public string? DbUser { get; set; }
    public string? DbPasswd { get; set; }
    public string? DbServerAndPort { get; set; }
}

public enum DatabaseType
{
    MySql,
    PostgreSql
}