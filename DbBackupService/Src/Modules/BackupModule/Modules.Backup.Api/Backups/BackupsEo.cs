using Microsoft.AspNetCore.Builder;

namespace Modules.Backup.Api.Backups;

internal static class BackupsEndpoints
{
    public static WebApplication MapBackupEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/backups");

        return app;
    }
}

internal abstract record BackupsOperations
{
}