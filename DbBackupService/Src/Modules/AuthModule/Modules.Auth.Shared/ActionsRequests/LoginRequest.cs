using System.ComponentModel.DataAnnotations;

namespace Modules.Auth.Shared.ActionsRequests;

public sealed class LoginRequest
{
    [Required] [EmailAddress] public string? Email { get; set; }

    [Required] public string? Password { get; set; }

    [Required] public bool RememberMe { get; set; } = true;
}