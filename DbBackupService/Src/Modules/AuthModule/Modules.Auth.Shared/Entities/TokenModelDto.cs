namespace Modules.Auth.Shared.Entities;

public sealed class TokenModelDto
{
    public string? Email { get; set; }
    public string? Token { get; set; }
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddHours(8);
}