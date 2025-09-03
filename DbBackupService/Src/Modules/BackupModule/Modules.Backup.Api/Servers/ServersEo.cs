using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Shared.Attributes;

namespace Modules.Backup.Api.Servers;

internal static class ServersEndpoints
{
    public static WebApplication MapServersEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/servers")
            .RequireAuthorization()
            .AddEndpointFilter<BasicTokenAuthorizationFilter>();

        
        
        return app;
    }
}

internal abstract record ServersOperations
{
}