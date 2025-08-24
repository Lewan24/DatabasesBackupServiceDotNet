using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using OneOf.Types;

namespace Modules.Shared.Attributes;

/// <summary>
///     Same as <see cref="BasicTokenAuthorizationAttribute" /> but with additional validation if user is in admin role
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminTokenAuthorizationAttribute : BasicTokenAuthorizationAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = CheckIfAllowAnonymousAndValidateToken(context);
        if (result.IsT2)
        {
            context.Result = new BadRequestObjectResult(result.AsT2);
            return;
        }

        if (result.IsT1)
        {
            var isAdmin = await IsUserAdmin(context);
            if (isAdmin.IsT1)
            {
                context.Result = new BadRequestObjectResult("Akcja wymaga uprawnień Administratora");
                return;
            }

            if (isAdmin.IsT2)
            {
                context.Result = new BadRequestObjectResult(isAdmin.AsT2);
                return;
            }
        }

        await next();
    }

    private async Task<OneOf<Yes, No, string>> IsUserAdmin(ActionExecutingContext context)
    {
        // TODO: Implement checking admin service
        //var adminApi = context.HttpContext.RequestServices.GetService<IAdminModuleApi>();

        // if (adminApi is null)
        //     return "Nie można uruchomić wymaganego serwisu";

        var userName = context.HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return "Nie można pobrać nazwy użytkownika";

        return new No();
        //return await adminApi.IsUserAdminAsync(userName, CancellationToken.None);
    }
}