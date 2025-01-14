using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Modules.Backup.Api;

internal static class Endpoints
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/backup");

        api.MapGet("Test", Operations.GetTestString)
            .WithSummary("Make Test Request")
            .WithDescription("Make a simple GET request to test API endpoint.")
            .AllowAnonymous();

        return app;
    }
}
