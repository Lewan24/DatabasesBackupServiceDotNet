using Microsoft.AspNetCore.Builder;

namespace Modules.Backup.Api.EOApp;

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
