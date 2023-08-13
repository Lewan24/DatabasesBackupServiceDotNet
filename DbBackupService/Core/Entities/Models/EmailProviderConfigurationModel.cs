namespace Core.Entities.Models;

public class EmailProviderConfigurationModel
{
    public bool EnableEmailProvider { get; set; } = true;
    public bool SendEmailOnEachDbSuccessfulBackup { get; set; }
    public bool SendEmailOnEachDbFailureBackup { get; set; } = true;
    public bool SendEmailWithStatisticsAfterBackups { get; set; } = true;
    public bool SendEmailOnOtherFailures { get; set; } = true;
    public EmailProviderCredentials EmailSenderCredentials { get; set; }
    public IQueryable<string> EmailReceivers { get; set; }
}

public record EmailProviderCredentials(string UserEmail, string Password, string SmtpHost, int SmtpPort = 587);