using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Modules.Backup.Api;

internal static class AppEndpoints
{
    public static WebApplication MapAppEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/app");

        api.MapGet("HealthCheck", () => "Not implemented");
        
        api.MapGet("ToggleEmailProvider", () => "Not implemented");
        
        return app;
    }
}
