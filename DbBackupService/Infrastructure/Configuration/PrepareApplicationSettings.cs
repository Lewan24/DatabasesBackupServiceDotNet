using Core.Entities.Models;
using Newtonsoft.Json;

namespace Infrastructure.Configuration;

public static class PrepareApplicationSettings
{
    public static Task<AppEmailConfigurationModel> Prepare(string? configurationJson)
    {
        try
        {
            if (configurationJson is null)
                throw new ArgumentNullException(nameof(configurationJson));

            return Task.FromResult(JsonConvert.DeserializeObject<AppEmailConfigurationModel>(configurationJson))!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Task.FromResult(new AppEmailConfigurationModel())!;
        }
    }
}