using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Administration.Shared.Interfaces;

namespace Modules.Shared.Attributes;

public class AdminTokenAuthorizationFilter(ILogger<AdminTokenAuthorizationFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        logger.LogInformation("Checking if bypass token validation is enabled");
        var allowWithoutValidation = context.HttpContext.GetEndpoint()?
            .Metadata.GetMetadata<AllowWithoutTokenValidationAttribute>() != null;

        if (allowWithoutValidation)
            return await next(context);
        
        logger.LogInformation("Validating token...");
        // Najpierw walidacja tokenu (z base)
        var tokenValidationResult = await BasicTokenAuthorizationFilter.ValidateTokenAndContinue(context, next);

        // Jeśli walidacja zwróciła cokolwiek innego niż next(context) → błąd
        if (tokenValidationResult is Microsoft.AspNetCore.Http.HttpResults.BadRequest)
            return tokenValidationResult;

        logger.LogInformation("Validating admin role...");
        // Sprawdź admina
        var adminApi = context.HttpContext.RequestServices.GetService<IAdminModuleApi>();
        if (adminApi is null)
            return Results.NotFound("Can't get admin api");

        var userName = context.HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Results.NotFound("Can't get user name");

        logger.LogInformation("Checking if user is admin...");
        var isAdmin = await adminApi.AmIAdmin(userName);
        
        if (!isAdmin)
            return Results.Forbid();

        // Jeśli admin OK → puszczamy dalej
        return await next(context);
    }
}