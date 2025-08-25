using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Modules.Backup.Api.EOTests;

internal static class TestsEndpoints
{
    public static WebApplication MapTestEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/tests");

        api.MapGet("TestGet", TestsOperations.GetTestString)
            .WithSummary("Make Test Request")
            .WithDescription("Make a simple GET request to test API endpoint.")
            .AllowAnonymous();

        api.MapGet("TestEmail", () => "Not implemented");

        return app;
    }
}