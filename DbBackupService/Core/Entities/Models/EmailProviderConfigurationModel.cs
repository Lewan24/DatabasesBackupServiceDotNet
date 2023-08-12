namespace Core.Entities.Models;

public class EmailProviderConfigurationModel
{
    public bool EnableEmailProvider { get; set; } = true;
    public bool SendEmailOnEachDbSuccessfulBackup { get; set; }
    public bool SendEmailOnEachDbFailureBackup { get; set; } = true;
    public bool SendEmailWithStatisticsAfterBackups { get; set; } = true;
    public EmailProviderCredentials EmailCredentials { get; set; } = new ("user@gmail.com", "Passwd", "smtp.gmail.com");
}

public record EmailProviderCredentials(string UserEmail, string Password, string SmtpHost, int SmtpPort = 587);