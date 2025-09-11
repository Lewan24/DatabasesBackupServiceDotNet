using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Backup.Shared.Dtos;
using Modules.Shared.Attributes;
using OneOf.Types;

namespace Modules.Backup.Api.Backups;

internal static class BackupsEndpoints
{
    public static WebApplication MapBackupEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/backups")
            .RequireAuthorization()
            .AddEndpointFilter<BasicTokenAuthorizationFilter>();

        api.MapGet("GetAllBackups", BackupsOperations.GetAllBackups)
            .WithSummary("Get all user's backups");
        
        api.MapPost("GetBackups", BackupsOperations.GetBackups)
            .WithSummary("Get user's 25 backups with provided offset");

        api.MapPost("PerformBackup", BackupsOperations.PerformBackup)
            .WithSummary("Perform backup on provided server");

        api.MapPost("DeleteBackup", BackupsOperations.DeleteBackup)
            .WithSummary("Delete backup entry + backup file")
            .AddEndpointFilter<AdminTokenAuthorizationFilter>();

        api.MapPost("DownloadBackup", BackupsOperations.DownloadBackup)
            .WithSummary("Download backup file from server");
        
        api.MapPost("TestBackup", BackupsOperations.TestBackup)
            .WithSummary("Test backup file in prepared test container");
        
        return app;
    }
}

internal abstract record BackupsOperations
{
    public static Task<IResult> GetAllBackups(
        HttpContext context)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(new List<PerformedBackupDto>()));
    }
    
    public static Task<IResult> GetBackups(
        HttpContext context,
        [FromBody] int offset = 0)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(new List<PerformedBackupDto>()));
    }

    public static Task<IResult> PerformBackup(
        HttpContext context,
        [FromBody] Guid serverId)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(new Success()));
    }

    public static Task<IResult> DeleteBackup(
        HttpContext context,
        [FromBody] Guid id)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(new Success()));
    }

    public static Task<IResult> DownloadBackup(
        HttpContext context,
        [FromBody] Guid id)
    {
        // TODO: return some type of stream or file?
        return Task.FromResult<IResult>(TypedResults.Ok(new Success()));
    }

    public static Task<IResult> TestBackup(
        HttpContext context,
        [FromBody] Guid id)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(new Success()));
    }
}