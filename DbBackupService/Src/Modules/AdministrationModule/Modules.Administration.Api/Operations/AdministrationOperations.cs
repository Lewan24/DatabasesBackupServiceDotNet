using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Administration.Application.Services;
using Modules.Administration.Shared.Interfaces;

namespace Modules.Administration.Api.Operations;

internal abstract class AdministrationOperations
{
    //TODO: Test these endpoints in client app 
    
    public static async Task<IResult> IsUserAdmin(
        HttpContext ctx,
        [FromServices] AdminService api,
        [FromBody] string username)
        => await api.IsUserAdmin(username);

    public static async Task AmIAdmin(
        HttpContext context, 
        [FromServices] IAdminModuleApi api)
        => await api.AmIAdmin(context.User.Identity?.Name);
}