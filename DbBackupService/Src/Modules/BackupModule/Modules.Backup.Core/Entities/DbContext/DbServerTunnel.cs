namespace Modules.Backup.Core.Entities.DbContext;

// TODO: Do sprawdzenia co jest wymagane do stworzenia i wlaczenia tunelu
public sealed record DbServerTunnel
{
    public Guid Id { get; init; }
    public required string ServerHost { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}