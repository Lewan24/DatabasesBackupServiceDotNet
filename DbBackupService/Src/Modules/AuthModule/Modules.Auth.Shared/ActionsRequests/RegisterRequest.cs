using System.ComponentModel.DataAnnotations;

namespace Modules.Auth.Shared.ActionsRequests;

public sealed class RegisterRequest
{
    [Required] [EmailAddress] public string? Email { get; set; }

    [Required] public string? Password { get; set; }

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Hasła się różnią!")]
    public string? PasswordConfirm { get; set; }
}