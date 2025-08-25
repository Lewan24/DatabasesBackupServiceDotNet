using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Modules.Auth.Shared.Interfaces;
using Modules.Auth.Shared.Static.Entities;

namespace Modules.Shared.Attributes;

public class BasicTokenAuthorizationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var allowWithoutValidation = context.HttpContext.GetEndpoint()?
            .Metadata.GetMetadata<AllowWithoutTokenValidationAttribute>() != null;

        if (allowWithoutValidation)
            return await next(context);

        var authToken = context.HttpContext.Request.Headers[AuthHeaderName.Name].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authToken))
        {
            return Results.BadRequest("Pusty token Autoryzacyjny. Nie można wykonać żądania. Zaloguj się ponownie, aby odświeżyć Token.");
        }

        var tokenValidationService = context.HttpContext.RequestServices.GetService<ITokenValidationService>();
        if (tokenValidationService is null)
        {
            return Results.BadRequest("Nie można uruchomić serwisu do sprawdzenia poprawności tokenu uwierzytelniającego.");
        }

        var isTokenValid = tokenValidationService.IsValid(authToken, context.HttpContext.User.Identity?.Name);

        return await isTokenValid.Match<ValueTask<object?>>(
            _ => next(context), // Token OK → przepuszczamy dalej
            _ => ValueTask.FromResult<object?>(Results.BadRequest("Nieprawidłowy token uwierzytelniający. Zaloguj się ponownie, aby odświeżyć Token."))
        );
    }
}