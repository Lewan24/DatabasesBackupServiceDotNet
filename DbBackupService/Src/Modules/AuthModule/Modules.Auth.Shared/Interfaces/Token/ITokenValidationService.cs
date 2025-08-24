using OneOf;
using OneOf.Types;

namespace Modules.Auth.Shared.Interfaces.Token;

public interface ITokenValidationService
{
    OneOf<True, False> IsValid(string? token, string? userEmail);
}