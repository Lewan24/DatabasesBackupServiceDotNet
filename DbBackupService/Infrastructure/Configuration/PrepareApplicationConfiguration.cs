using Core.Entities;
using Newtonsoft.Json;

namespace Infrastructure.Configuration;

public static class PrepareApplicationConfiguration
{
    public static async Task<ApplicationConfiguration?> Prepare(string? configurationJson)
    {
        try
        {
            if (configurationJson is null)
                throw new ArgumentNullException(nameof(configurationJson));

            return JsonConvert.DeserializeObject<ApplicationConfiguration?>(configurationJson);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new ApplicationConfiguration();
        }
    }
}