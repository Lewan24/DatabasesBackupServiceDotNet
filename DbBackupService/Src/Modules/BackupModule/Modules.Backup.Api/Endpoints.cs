using Microsoft.AspNetCore.Builder;

namespace Modules.Backup.Api;

internal static class Endpoints
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/backup");



        return app;
    }
}
