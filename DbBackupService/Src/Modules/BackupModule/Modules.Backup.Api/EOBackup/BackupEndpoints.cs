using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Modules.Backup.Api;

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
