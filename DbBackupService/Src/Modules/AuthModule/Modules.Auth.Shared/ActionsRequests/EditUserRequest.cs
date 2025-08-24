using System.ComponentModel.DataAnnotations;

namespace Modules.Auth.Shared.ActionsRequests;

public sealed record EditUserRequest
{
    public required string Id { get; set; }

    [EmailAddress] [MaxLength(100)] public string? Email { get; set; }

    [MaxLength(40)] public string? Password { get; set; }

    [MaxLength(40)]
    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }
}