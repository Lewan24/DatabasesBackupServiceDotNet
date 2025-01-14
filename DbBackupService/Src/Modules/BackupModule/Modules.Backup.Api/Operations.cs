using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Modules.Backup.Api;

internal abstract record Operations
{
    public static async Task<Results<Ok<string>, BadRequest<string>>> GetTestString(HttpContext context, 
        [FromServices] ILogger<Operations> logger,
        [FromQuery] bool failRequest = false)
    {
        logger.LogInformation("Executing GetTestString operation.");

        return failRequest switch
        {
            true => TypedResults.BadRequest("Got Fail Command. This is a Test BadRequest."),
            _ => TypedResults.Ok("Successfully Tested Api Endpoint. Thanks for using the Backup Service.")
        };
    }
}
