using Microsoft.AspNetCore.Builder;

namespace Modules.Backup.Api.Backups;

internal static class BackupsEndpoints
{
    public static WebApplication MapBackupEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/backup");

        api.MapGet("MakeBackup", () => "Not implemented");
        api.MapGet("GetBackupPath", () => "Not implemented");
        api.MapGet("GetBackupsList", () => "Not implemented");

        return app;
    }
}

internal abstract record BackupsOperations
{
}