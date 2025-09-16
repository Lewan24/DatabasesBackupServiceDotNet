using System.ComponentModel.DataAnnotations;

namespace Modules.Auth.Shared.ActionsRequests;

public sealed class ChangePasswordRequest
{
    [Required] [MaxLength(40)] public string CurrentPassword { get; set; } = null!;

    [Required] [MaxLength(40)] public string NewPassword { get; set; } = null!;

    [Required]
    [MaxLength(40)]
    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } = null!;
}