using Modules.Auth.Shared.Entities.Tokens;

namespace Modules.Auth.Infrastructure.Repositories;

public interface IUserTokenService
{
    IEnumerable<TokenModelDto> GetUserTokens(Func<TokenModelDto, bool> predicate);
}