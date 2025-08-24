namespace Modules.Auth.Shared.Dtos;

public sealed class UserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public IList<string>? Roles { get; set; }
}