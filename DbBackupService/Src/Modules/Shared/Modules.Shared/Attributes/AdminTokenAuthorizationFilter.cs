using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Administration.Shared.Interfaces;

namespace Modules.Shared.Attributes;

public class AdminTokenAuthorizationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var allowWithoutValidation = context.HttpContext.GetEndpoint()?
            .Metadata.GetMetadata<AllowWithoutTokenValidationAttribute>() != null;

        if (allowWithoutValidation)
            return await next(context);
        
        // Najpierw walidacja tokenu (z base)
        var tokenValidationResult = await BasicTokenAuthorizationFilter.ValidateTokenAndContinue(context, next);
        if (tokenValidationResult.IsT1)
            return await new ValueTask<object?>(Results.BadRequest(tokenValidationResult.AsT1));

        // Sprawdź admina
        var adminApi = context.HttpContext.RequestServices.GetService<IAdminModuleApi>();
        if (adminApi is null)
            return Results.NotFound("Can't get admin api");

        var userName = context.HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Results.NotFound("Can't get user name");

        var isAdmin = await adminApi.AmIAdmin(userName);
        
        if (!isAdmin)
            return Results.Forbid();

        // Jeśli admin OK → puszczamy dalej
        return await next(context);
    }
}