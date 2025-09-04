using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Backup.Application.Services;
using Modules.Backup.Shared.Dtos;
using Modules.Backup.Shared.Requests;
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
        
        api.MapPost("CreateServer", ServersOperations.CreateServer)
            .WithSummary("Create a new server");

        api.MapPost("EditServer", ServersOperations.EditServer)
            .WithSummary("Edit existing server");

        api.MapPost("ToggleServerDisabledStatus", ServersOperations.ToggleServerDisabledStatus)
            .WithSummary("Toggle server's disabled status");

        api.MapGet("GetServersUsers", ServersOperations.GetServersUsers)
            .WithSummary("Get servers and users that access server")
            .AddEndpointFilter<AdminTokenAuthorizationFilter>();

        api.MapPost("GetUsersThatAccessServer", ServersOperations.GetUsersThatAccessServer)
            .WithSummary("Get users that access specified server")
            .AddEndpointFilter<AdminTokenAuthorizationFilter>();
        
        api.MapPost("GetAllUsersThatDoesNotHaveAccessToServer", ServersOperations.GetAllUsersThatDoesNotHaveAccessToServer)
            .WithSummary("Get users that does not access specified server")
            .AddEndpointFilter<AdminTokenAuthorizationFilter>();
        
        api.MapPost("RemoveUserAccessFromServer", ServersOperations.RemoveUserAccessFromServer)
            .WithSummary("Remove access to server from user")
            .AddEndpointFilter<AdminTokenAuthorizationFilter>();
        
        api.MapPost("GiveUserAccessToServer", ServersOperations.GiveUserAccessToServer)
            .WithSummary("Grant user access to server")
            .AddEndpointFilter<AdminTokenAuthorizationFilter>();
        
        return app;
    }
}

internal abstract class ServersOperations
{
    public static async Task<IResult> GetUserServers(
        HttpContext context,
        [FromServices] ServersService service)
    {
        var result = await service.GetServers(context.User.Identity?.Name);

        return result.Match<IResult>(
            servers => TypedResults.Ok(servers),
            error => TypedResults.BadRequest(error)
        );
    }

    public static async Task<IResult> CreateServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] ServerConnectionDto newServer)
    {
        var result = await service.CreateServer(newServer, context.User.Identity?.Name);

        return result.Match<IResult>(
            _ => TypedResults.Created(),
            error => TypedResults.BadRequest(error)
        );
    }

    public static async Task<IResult> EditServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] ServerConnectionDto server)
    {
        var result = await service.EditServer(server);

        return result.Match<IResult>(
            _ => TypedResults.Ok(),
            error => TypedResults.BadRequest(error)
        );
    }

    public static async Task<IResult> ToggleServerDisabledStatus(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] Guid serverId)
    {
        var result = await service.ToggleDisabledStatus(serverId, context.User.Identity!.Name);
        
        return result.Match<IResult>(
            _ => TypedResults.Ok(),
            error => TypedResults.BadRequest(error)
        );
    }

    public static async Task<IResult> GetServersUsers(
        HttpContext context,
        [FromServices] ServersService service)
        => TypedResults.Ok(await service.GetServersUsers());

    public static async Task<IResult> GetUsersThatAccessServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] Guid serverId)
    {
        var result = await service.GetUsersThatAccessServer(serverId);
        
        return result.Match<IResult>(
            list => TypedResults.Ok(list),
            error => TypedResults.BadRequest(error)
        );
    }

    public static async Task<IResult> GetAllUsersThatDoesNotHaveAccessToServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] Guid serverId)
    {
        var result = await service.GetAllUsersThatDoesNotHaveAccessToServer(serverId);
        
        return result.Match<IResult>(
            list => TypedResults.Ok(list),
            error => TypedResults.BadRequest(error)
        );
    }

    public static async Task<IResult> RemoveUserAccessFromServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] ModifyServerAccessRequest request)
    {
        var result = await service.RemoveUserAccessFromServer(request);
        
        return result.Match<IResult>(
            _ => TypedResults.Ok(),
            error => TypedResults.BadRequest(error)
        );
    }

    public static async Task<IResult> GiveUserAccessToServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] ModifyServerAccessRequest request)
    {
        var result = await service.GiveUserAccessToServer(request);
        
        return result.Match<IResult>(
            _ => TypedResults.Ok(),
            error => TypedResults.BadRequest(error)
        );
    }
}