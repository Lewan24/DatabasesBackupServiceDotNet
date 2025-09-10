using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Backup.Application.Services;
using Modules.Backup.Shared.Dtos;
using Modules.Backup.Shared.Requests;
using Modules.Shared.Attributes;
using OneOf;
using OneOf.Types;

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
        
        api.MapGet("GetMyServersForSchedule", ServersOperations.GetServersForSchedule)
            .WithSummary("Get user's servers for schedule");
        
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

        api.MapPost("TestServerConnection", ServersOperations.TestServerConnection)
            .WithSummary("Test server connection and execute test query to check communication with DB");
        
        return app;
    }
}

internal abstract class ServersOperations
{
    private static async Task<IResult> CallFuncAndReturnIResult<TSuccess, TError, TData>(Func<TData, Task<OneOf<TSuccess, TError>>> func, TData data)
    {
        var result = await func(data);
        
        return result.Match<IResult>(
            TypedResults.Ok,
            TypedResults.BadRequest
        );
    }
    
    public static async Task<IResult> GetUserServers(
        HttpContext context,
        [FromServices] ServersService service)
        => await CallFuncAndReturnIResult(service.GetServers, context.User.Identity?.Name);

    public static async Task<IResult> GetServersForSchedule(
        HttpContext context,
        [FromServices] ServersService service)
        => await CallFuncAndReturnIResult(service.GetAvailableServersBasic, context.User.Identity?.Name);

    public static async Task<IResult> CreateServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] ServerConnectionDto newServer)
    {
        var result = await service.CreateServer(newServer, context.User.Identity?.Name);

        return result.Match<IResult>(
            _ => TypedResults.Created(),
            TypedResults.BadRequest
        );
    }

    public static async Task<IResult> EditServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] ServerConnectionDto server)
        => await CallFuncAndReturnIResult(service.EditServer, server);

    public static async Task<IResult> ToggleServerDisabledStatus(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] Guid serverId)
    {
        var result = await service.ToggleDisabledStatus(serverId, context.User.Identity!.Name);
        
        return result.Match<IResult>(
            TypedResults.Ok,
            TypedResults.BadRequest
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
        => await CallFuncAndReturnIResult(service.GetUsersThatAccessServer, serverId);

    public static async Task<IResult> GetAllUsersThatDoesNotHaveAccessToServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] Guid serverId)
        => await CallFuncAndReturnIResult(service.GetAllUsersThatDoesNotHaveAccessToServer, serverId);

    public static async Task<IResult> RemoveUserAccessFromServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] ModifyServerAccessRequest request)
        => await CallFuncAndReturnIResult(service.RemoveUserAccessFromServer, request);

    public static async Task<IResult> GiveUserAccessToServer(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] ModifyServerAccessRequest request)
        => await CallFuncAndReturnIResult(service.GiveUserAccessToServer, request);

    public static async Task<IResult> TestServerConnection(
        HttpContext context,
        [FromServices] ServersService service,
        [FromBody] Guid serverId)
        => await CallFuncAndReturnIResult(service.TestServerConnection, serverId);
}