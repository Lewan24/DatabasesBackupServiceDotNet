using Core.Entities.Models;
using Newtonsoft.Json;

namespace Infrastructure.Configuration;

public static class PrepareApplicationConfiguration
{
    public static Task<ApplicationConfigurationModel?> Prepare(string? configurationJson)
    {
        try
        {
            if (configurationJson is null)
                throw new ArgumentNullException(nameof(configurationJson));

            return Task.FromResult(JsonConvert.DeserializeObject<ApplicationConfigurationModel?>(configurationJson));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Task.FromResult(new ApplicationConfigurationModel())!;
        }
    }
}