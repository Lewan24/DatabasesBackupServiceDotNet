using OneOf;
using OneOf.Types;

namespace Modules.Auth.Shared.Interfaces.Health;

public interface IAuthHealthService
{
    Task<OneOf<True, False, Error>> CheckHealthAsync();
}