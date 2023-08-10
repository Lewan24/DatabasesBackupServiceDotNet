namespace Core.Models;

public class DatabaseConfigModel
{
    // TODO: Add other properties like dbserver, user passwd etc
    public DatabaseType DbType { get; set; } = DatabaseType.MySql;
    public string? DbName { get; set; }
}

public enum DatabaseType
{
    MySql,
    PostgreSql
}