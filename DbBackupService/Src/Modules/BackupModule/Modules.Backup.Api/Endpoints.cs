using Microsoft.AspNetCore.Builder;

namespace Modules.Backup.Api;

public static class Endpoints
{
    public static WebApplication MapBackupEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/backup");



        return app;
    }
}
