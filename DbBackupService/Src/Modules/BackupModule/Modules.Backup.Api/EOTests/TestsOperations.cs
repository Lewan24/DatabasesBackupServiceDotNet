using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Modules.Backup.Api.EOTests;

internal abstract record TestsOperations
{
    public static async Task<Results<Ok<string>, BadRequest<string>>> GetTestString(HttpContext context,
        [FromServices] ILogger<TestsOperations> logger,
        [FromQuery] bool failRequest = false)
    {
        logger.LogInformation("Executing GetTestString operation.");

        await Task.Delay(500);
        
        return failRequest switch
        {
            true => TypedResults.BadRequest("Got Fail Command. This is a Test BadRequest."),
            _ => TypedResults.Ok("Successfully Tested Api Endpoint. Thanks for using the Backup Service.")
        };
    }
}