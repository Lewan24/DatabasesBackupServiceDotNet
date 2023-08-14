namespace Core.Entities.Models;

public class AppEmailConfigurationModel
{
    public ApplicationConfigurationModel AppConfiguration { get; set; }
    public EmailProviderConfigurationModel EmailProviderConfiguration { get; set; }
}