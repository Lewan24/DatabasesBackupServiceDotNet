namespace Core.Entities.Models;

public class EmailProviderConfigurationModel
{
    public EmailSettings ProviderSettings { get; set; }
    public EmailProviderCredentials EmailSenderCredentials { get; set; }
    public List<string> EmailReceivers { get; set; }
}

public record EmailProviderCredentials(string EmailSender, string EmailSenderDisplayName, string Password, string SmtpHost, int SmtpPort = 587);

public record EmailSettings(bool EnableEmailProvider = true, bool UseStartTls = true, bool UseSslInstead = false, 
    bool SendEmailOnEachDbSuccessfulBackup = false, bool SendEmailOnEachDbFailureBackup = true,
    bool SendEmailWithStatisticsAfterBackups = true, bool SendEmailOnOtherFailures = true);