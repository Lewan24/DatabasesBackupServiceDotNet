using System.Net;
using Microsoft.Extensions.Logging;
using Modules.Auth.Shared.Interfaces.Health;
using OneOf;
using OneOf.Types;

namespace Modules.Auth.Application.Services.Health;

internal sealed class AuthHealthService(HttpClient httpClient, ILogger<AuthHealthService> logger) : IAuthHealthService
{
    public async Task<OneOf<True, False, Error>> CheckHealthAsync()
    {
        try
        {
            var result = await httpClient.GetAsync("_health");
            if (result.StatusCode is HttpStatusCode.OK)
                return new True();

            return new False();
        }
        catch (Exception e)
        {
            logger.LogWarning("Error has been thrown during checking _health state for Auth. Error: {Error}", e.Message);

            return new Error();
        }
    }
}