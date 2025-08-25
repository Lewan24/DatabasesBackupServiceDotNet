using Modules.Auth.Infrastructure.DbContexts;
using Modules.Auth.Shared.Entities;

namespace Modules.Auth.Infrastructure.Repositories;

internal class UserTokenService(AppIdentityDbContext context) : IUserTokenService
{
    public IEnumerable<TokenModelDto> GetUserTokens(Func<TokenModelDto, bool> predicate)
    {
        var userTokensDtos = context.UsersTokens
            .Select(ut => new TokenModelDto
            {
                Email = ut.Email,
                Token = ut.Token,
                ExpirationDate = ut.ExpirationDate
            })
            .AsEnumerable()
            .Where(predicate);

        return userTokensDtos;
    }
}