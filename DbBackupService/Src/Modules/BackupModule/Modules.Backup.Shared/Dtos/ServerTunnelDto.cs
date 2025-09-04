using System.ComponentModel.DataAnnotations;

namespace Modules.Backup.Shared.Dtos;

public record ServerTunnelDto
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// Host serwera SSH (np. domain.com)
    /// </summary>
    [Required] public string? ServerHost { get; set; }

    /// <summary>
    /// Port serwera SSH (domyślnie 22, w twoim przykładzie 80)
    /// </summary>
    [Required] public int SshPort { get; set; } = 22;

    /// <summary>
    /// Użytkownik SSH (np. user)
    /// </summary>
    [Required] public string? Username { get; set; }

    /// <summary>
    /// Czy używać hasła do logowania (true) czy klucza prywatnego (false)
    /// </summary>
    public bool UsePasswordAuth { get; set; } = true;

    /// <summary>
    /// Hasło użytkownika (jeśli logowanie hasłem)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Zawartość pliku z kluczem prywatnym (jeśli logowanie kluczem)
    /// </summary>
    public string? PrivateKeyContent { get; set; }

    /// <summary>
    /// Lokalny port, na którym wystawiony będzie tunel (np. 8543)
    /// </summary>
    [Required] public int LocalPort { get; set; }

    /// <summary>
    /// Zdalny host, do którego tunel ma przekierowywać (np. pgsqldomain.com)
    /// </summary>
    [Required] public string? RemoteHost { get; set; }

    /// <summary>
    /// Zdalny port usługi (np. 5432 dla PostgreSQL, 3306 dla MySQL)
    /// </summary>
    [Required] public int RemotePort { get; set; }

    /// <summary>
    /// Opis tunelu (opcjonalny, do UI)
    /// </summary>
    public string? Description { get; set; }
}