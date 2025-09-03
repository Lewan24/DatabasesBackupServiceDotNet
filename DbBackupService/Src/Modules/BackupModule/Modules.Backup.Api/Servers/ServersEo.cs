using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Backup.Application.Services;
using Modules.Shared.Attributes;

namespace Modules.Backup.Api.Servers;

internal static class ServersEndpoints
{
    public static WebApplication MapServersEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/servers")
            .RequireAuthorization()
            .AddEndpointFilter<BasicTokenAuthorizationFilter>();

        api.MapGet("GetMyServers", ServersOperations.GetUserServers)
            .WithSummary("Get user's enabled servers");
        
        return app;
    }
}

internal abstract record ServersOperations
{
    public static async Task GetUserServers(
        HttpContext context,
        [FromServices] ServersService service)
    => await service.GetServers(context.User.Identity?.Name);
}