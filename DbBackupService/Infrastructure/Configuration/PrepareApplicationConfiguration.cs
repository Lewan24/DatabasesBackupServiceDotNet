using System;
using System.Threading.Tasks;
using Core.Entities;
using Newtonsoft.Json;

namespace Infrastructure.Configuration;

public static class PrepareApplicationConfiguration
{
    public static Task<ApplicationConfiguration?> Prepare(string? configurationJson)
    {
        try
        {
            if (configurationJson is null)
                throw new ArgumentNullException(nameof(configurationJson));

            return Task.FromResult(JsonConvert.DeserializeObject<ApplicationConfiguration?>(configurationJson));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Task.FromResult(new ApplicationConfiguration())!;
        }
    }
}