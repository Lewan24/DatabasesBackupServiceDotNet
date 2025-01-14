using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Modules.Backup.Api;

internal static class Endpoints
{
    public static WebApplication MapConfigEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/config");

        api.MapGet("GetDbsConnectionsConfig", () => "Not implemented");
        api.MapPut("EditDbsConnections", () => "Not implemented");
        api.MapPost("AddDbConnectionToConfig", () => "Not implemented");
        
        api.MapGet("GetAppConfig", () => "Not implemented");
        api.MapPut("EditAppConfig", () => "Not implemented");
        
        api.MapGet("GetEmailProviderConfig", () => "Not implemented");
        api.MapPut("EditEmailProviderConfig", () => "Not implemented");
        
        api.MapGet("ReloadConfigs", () => "Not implemented");
        
        return app;
    }
    
    public static WebApplication MapAppEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/app");

        api.MapGet("HealthCheck", () => "Not implemented");
        
        api.MapGet("ToggleEmailProvider", () => "Not implemented");
        
        return app;
    }
    
    public static WebApplication MapBackupEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/backup");

        api.MapGet("MakeBackup", () => "Not implemented");
        api.MapGet("GetBackupPath", () => "Not implemented");
        api.MapGet("GetBackupsList", () => "Not implemented");
        
        return app;
    }
    
    public static WebApplication MapTestEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/tests");

        api.MapGet("TestGet", Operations.GetTestString)
            .WithSummary("Make Test Request")
            .WithDescription("Make a simple GET request to test API endpoint.")
            .AllowAnonymous();

        api.MapGet("TestEmail", () => "Not implemented");

        return app;
    }
}
