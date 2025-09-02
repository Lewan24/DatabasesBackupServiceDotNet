using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Auth.Shared.Interfaces;
using Modules.Auth.Shared.Static.Entities;
using OneOf;
using OneOf.Types;

namespace Modules.Shared.Attributes;

public class BasicTokenAuthorizationFilter(ILogger<BasicTokenAuthorizationFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var allowWithoutValidation = context.HttpContext.GetEndpoint()?
            .Metadata.GetMetadata<AllowWithoutTokenValidationAttribute>() != null;

        if (allowWithoutValidation)
            return await next(context);
        
        var validationResult = await ValidateTokenAndContinue(context, next);
        return validationResult.Match(
            _ => next(context),
            error => new ValueTask<object?>(Results.BadRequest(error)));
    }

    /// <summary>
    /// Wspólna logika walidacji tokenu do ponownego użycia w klasach dziedziczących.
    /// </summary>
    public static async Task<OneOf<Success, string>> ValidateTokenAndContinue(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var authToken = context.HttpContext.Request.Headers[AuthHeaderName.Name].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authToken))
            return "Empty authorization token. The request cannot be executed. Please log in again to refresh the token.";

        var tokenValidationService = context.HttpContext.RequestServices.GetService<ITokenValidationService>();
        if (tokenValidationService is null)
            return "The service for verifying the authenticity of the authentication token cannot be started. Try to restart application.";

        var isTokenValid = tokenValidationService.IsValid(authToken, context.HttpContext.User.Identity?.Name);

        return isTokenValid.Match<OneOf<Success, string>>(
            _ => new Success(),
            _ => "Invalid authentication token. Please log in again to refresh the token."
        );
    }
}