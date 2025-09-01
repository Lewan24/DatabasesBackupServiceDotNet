using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Administration.Api.Operations;
using Modules.Shared.Attributes;

namespace Modules.Administration.Api.Endpoints;

internal static class AdministrationEndpoints
{
    public static WebApplication MapAdministrationEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/administration")
            .RequireAuthorization()
            .AddEndpointFilter(new AdminTokenAuthorizationFilter());

        api.MapGet("AmIAdmin", AdministrationOperations.AmIAdmin)
            .WithSummary("Check if the user is admin")
            .WithMetadata(new AllowWithoutTokenValidationAttribute());

        api.MapGet("IsUserAdmin", AdministrationOperations.IsUserAdmin)
            .WithSummary("Check if the specified user is admin");
        
        return app;
    }
}