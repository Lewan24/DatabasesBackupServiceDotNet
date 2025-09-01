using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Modules.Administration.Shared.Interfaces;

namespace Modules.Shared.Attributes;

public class AdminTokenAuthorizationFilter : BasicTokenAuthorizationFilter
{
    public new async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var allowWithoutValidation = context.HttpContext.GetEndpoint()?
            .Metadata.GetMetadata<AllowWithoutTokenValidationAttribute>() != null;

        if (allowWithoutValidation)
            return await next(context);
        
        // Najpierw walidacja tokenu (z base)
        var tokenValidationResult = await ValidateTokenAndContinue(context, next);

        // Jeśli walidacja zwróciła cokolwiek innego niż next(context) → błąd
        if (tokenValidationResult is IResult and not Microsoft.AspNetCore.Http.HttpResults.Ok)
            return tokenValidationResult;

        // Sprawdź admina
        var adminApi = context.HttpContext.RequestServices.GetService<IAdminModuleApi>();
        if (adminApi is null)
            return Results.NotFound("Can't get admin api");

        var userName = context.HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Results.NotFound("Can't get user name");

        var isAdminResult = await adminApi.AmIAdmin(userName);
        if (isAdminResult is not Microsoft.AspNetCore.Http.HttpResults.Ok)
            return Results.Forbid();

        // Jeśli admin OK → puszczamy dalej
        return await next(context);
    }
}