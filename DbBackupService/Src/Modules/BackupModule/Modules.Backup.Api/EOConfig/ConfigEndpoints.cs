using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Modules.Backup.Api;

internal static class ConfigEndpoints
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
}
