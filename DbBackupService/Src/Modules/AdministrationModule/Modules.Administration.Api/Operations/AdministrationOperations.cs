using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Administration.Application.Services;
using Modules.Administration.Shared.Interfaces;

namespace Modules.Administration.Api.Operations;

internal abstract class AdministrationOperations
{
    public static async Task<IResult> IsUserAdmin(
        HttpContext ctx,
        [FromServices] AdminService api,
        [FromBody] string username)
    {
        var result = await api.IsUserAdmin(username);

        return result switch
        {
            null => TypedResults.NotFound("User does not exist"),
            _ => TypedResults.Ok(result)
        };
    }

    public static async Task<IResult> AmIAdmin(
        HttpContext context,
        [FromServices] IAdminModuleApi api)
        => TypedResults.Ok(await api.AmIAdmin(context.User.Identity?.Name));
}