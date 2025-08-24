using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Modules.Auth.Shared.Interfaces.Token;
using Modules.Auth.Shared.Static.Entities;
using OneOf;
using OneOf.Types;

namespace Modules.Shared.Attributes;

/// <summary>
///     Specifies that the class or method needs to be validated with token directed in auth header before request
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BasicTokenAuthorizationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var result = CheckIfAllowAnonymousAndValidateToken(context);
        
        if (result.IsT2)
        {
            context.Result = new BadRequestObjectResult(result.AsT2);
            return;
        }

        base.OnActionExecuting(context);
    }

    protected OneOf<Yes, No, string> CheckIfAllowAnonymousAndValidateToken(ActionExecutingContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowWithoutTokenValidationAttribute>()
            .Any();

        if (allowAnonymous) 
            return new Yes();
        
        var validationResult = IsTokenValid(context);

        if (validationResult.IsT1)
            return validationResult.AsT1;

        return new No();
    }

    private OneOf<Success, string> IsTokenValid(ActionExecutingContext context)
    {
        var authToken = context.HttpContext.Request.Headers[AuthHeaderName.Name].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authToken))
            return "Pusty token Autoryzacyjny. Nie można wykonać żądania. Zaloguj się ponownie, aby odświeżyć Token.";

        var tokenValidationService =
            (ITokenValidationService?)context.HttpContext.RequestServices.GetService(typeof(ITokenValidationService));
        if (tokenValidationService is null)
            return "Nie można uruchomić serwisu do sprawdzenia poprawności tokenu uwierzytelniającego.";

        var isTokenValid = tokenValidationService.IsValid(authToken, context.HttpContext.User.Identity?.Name);

        return isTokenValid.Match<OneOf<Success, string>>
        (
            _ => new Success(),
            _ => "Nieprawidłowy token uwierzytelniający. Zaloguj się ponownie, aby odświeżyć Token."
        );
    }
}