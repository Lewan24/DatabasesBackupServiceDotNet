using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Backup.Infrasctructure.Interfaces;

namespace Modules.Backup.Api;

internal abstract record TestsOperations
{
    public static async Task<Results<Ok<string>, BadRequest<string>>> GetTestString(HttpContext context,
        [FromServices] ILogger<TestsOperations> logger, [FromServices] IDbScheduleRepo repo,
        [FromQuery] bool failRequest = false)
    {
        logger.LogInformation("Executing GetTestString operation.");

        await repo.SaveChanges();

        return failRequest switch
        {
            true => TypedResults.BadRequest("Got Fail Command. This is a Test BadRequest."),
            _ => TypedResults.Ok("Successfully Tested Api Endpoint. Thanks for using the Backup Service.")
        };
    }
}