using Microsoft.AspNetCore.Builder;

namespace Modules.Backup.Api.EOBackup;

internal static class BackupEndpoints
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