using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Auth.Shared.Interfaces;
using Modules.Auth.Shared.Static.Entities;

namespace Modules.Shared.Attributes;

public class BasicTokenAuthorizationFilter(ILogger<BasicTokenAuthorizationFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        logger.LogInformation("Checking if bypass token validation is enabled");
        var allowWithoutValidation = context.HttpContext.GetEndpoint()?
            .Metadata.GetMetadata<AllowWithoutTokenValidationAttribute>() != null;

        if (allowWithoutValidation)
            return await next(context);
        
        logger.LogInformation("Validating token...");
        return await ValidateTokenAndContinue(context, next);
    }

    /// <summary>
    /// Wspólna logika walidacji tokenu do ponownego użycia w klasach dziedziczących.
    /// </summary>
    public static async ValueTask<object?> ValidateTokenAndContinue(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var authToken = context.HttpContext.Request.Headers[AuthHeaderName.Name].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authToken))
            return Results.BadRequest(
                "Empty authorization token. The request cannot be executed. Please log in again to refresh the token.");

        var tokenValidationService = context.HttpContext.RequestServices.GetService<ITokenValidationService>();
        if (tokenValidationService is null)
            return Results.BadRequest(
                "The service for verifying the authenticity of the authentication token cannot be started. Try to restart application.");

        var isTokenValid = tokenValidationService.IsValid(authToken, context.HttpContext.User.Identity?.Name);

        return await isTokenValid.Match(
            _ => next(context), // Token OK → przepuszczamy dalej
            _ => ValueTask.FromResult<object?>(
                Results.BadRequest("Invalid authentication token. Please log in again to refresh the token."))
        );
    }
}