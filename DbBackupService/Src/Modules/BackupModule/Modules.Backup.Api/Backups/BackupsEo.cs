using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Backup.Application.Interfaces;
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

        api.MapPost("PerformSchedulesBackup", BackupsOperations.PerformSchedulesBackup)
            .WithSummary("Perform backup on pending schedules")
            .AddEndpointFilter<AdminTokenAuthorizationFilter>();

        api.MapPost("DeleteBackup", BackupsOperations.DeleteBackup)
            .WithSummary("Delete backup entry + backup file")
            .AddEndpointFilter<AdminTokenAuthorizationFilter>();

        api.MapGet("Download", BackupsOperations.DownloadBackup)
            .WithSummary("Download backup file from server")
            .WithMetadata(new AllowWithoutTokenValidationAttribute());

        api.MapPost("TestBackup", BackupsOperations.TestBackup)
            .WithSummary("Test backup file in prepared test container");

        return app;
    }
}

internal abstract record BackupsOperations
{
    public static async Task<IResult> GetAllBackups(
        HttpContext context,
        [FromServices] IDbBackupService service)
    {
        var result = await service.GetAllBackups(context.User.Identity?.Name);

        return result.Match<IResult>(
            TypedResults.Ok,
            TypedResults.BadRequest
        );
    }

    public static Task<IResult> GetBackups(
        HttpContext context,
        [FromBody] int offset = 0)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(new List<PerformedBackupDto>()));
    }

    public static async Task<IResult> PerformBackup(
        HttpContext context,
        [FromServices] IDbBackupService service,
        [FromBody] Guid serverId)
    {
        var result = await service.BackupDb(serverId);

        return result.Match<IResult>(
            TypedResults.Ok,
            TypedResults.BadRequest
        );
    }

    // TODO: idk why but AdminTokenFilter needs additional parameter to provide
    public static Task<IResult> PerformSchedulesBackup(
        HttpContext context,
        [FromBody] bool test)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(new Success()));
    }

    public static Task<IResult> DeleteBackup(
        HttpContext context,
        [FromBody] Guid id)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(new Success()));
    }

    public static async Task<IResult> DownloadBackup(
        HttpContext context,
        [FromServices] IDbBackupService service,
        [FromQuery] Guid id)
    {
        var result = await service.DownloadBackup(id, context.User.Identity?.Name);

        return result.Match<IResult>(
            file => TypedResults.PhysicalFile(file.FilePath, file.ContentType, file.FileName),
            TypedResults.BadRequest
        );
    }

    public static Task<IResult> TestBackup(
        HttpContext context,
        [FromBody] Guid id)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(new Success()));
    }
}