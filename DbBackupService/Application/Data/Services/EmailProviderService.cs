using Application.Data.Interfaces;
using Core.Entities.Models;
using NLog;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Application.Data.Services;

public class EmailProviderService : IEmailProviderService
{
    private readonly EmailProviderConfigurationModel _configuration;
    private readonly Logger _logger;

    private readonly SmtpClient _smtpClient = new ();

    public EmailProviderService (EmailProviderConfigurationModel configuration, Logger logger)
    {
        _configuration = configuration;
        _logger = logger.Factory.GetLogger(nameof(EmailProviderService));
    }

    public async Task PrepareAndSendEmail(MailModel mailRequest)
    {
        if (!_configuration.ProviderSettings.EnableEmailProvider)
        {
            _logger.Info("Can't send email. EmailProvider disabled in settings");
            return;
        }

        _logger.Info("Preparing email provider and its components...");

        try
        {
            var mail = PrepareMessage(mailRequest);
            if (mail is null)
                throw new ArgumentNullException(nameof(mail), "Can't prepare message to send");

            await SendEmail(mail);
        }
        catch (ArgumentNullException e)
        {
            _logger.Warn(e, "Error thrown while preparing mail message.");
        }
        catch (Exception e)
        {
            _logger.Warn(e);
        }
        finally
        {
            _logger.Info("Email request had been handled.");
        }
    }

    private MimeMessage? PrepareMessage(MailModel request)
    {
        _logger.Info("Preparing mail message...");
        var mail = new MimeMessage();

        mail.Subject = request.Subject;
        mail.Body = new TextPart("html")
        {
            Text = request.Body
        };

        mail.From.Add(new MailboxAddress(_configuration.EmailSenderCredentials.EmailSenderDisplayName, _configuration.EmailSenderCredentials.EmailSender));

        if (!_configuration.EmailReceivers.Any())
        {
            _logger.Warn("Receivers list can't be empty");
            return null;
        }

        foreach (var receiver in _configuration.EmailReceivers)
            mail.To.Add(new MailboxAddress(receiver, receiver));

        return mail;
    }

    private async Task SendEmail(MimeMessage mail)
    {
        try
        {
            _logger.Info("Connecting to mail server...");
            if (_configuration.ProviderSettings.UseStartTls)
                await _smtpClient.ConnectAsync(_configuration.EmailSenderCredentials.SmtpHost,
                    _configuration.EmailSenderCredentials.SmtpPort, SecureSocketOptions.StartTls);
            else if (_configuration.ProviderSettings.UseSslInstead)
                await _smtpClient.ConnectAsync(_configuration.EmailSenderCredentials.SmtpHost,
                    _configuration.EmailSenderCredentials.SmtpPort, _configuration.ProviderSettings.UseSslInstead);
            else
                await _smtpClient.ConnectAsync(_configuration.EmailSenderCredentials.SmtpHost,
                    _configuration.EmailSenderCredentials.SmtpPort);

            _logger.Info("Authentication...");
            await _smtpClient.AuthenticateAsync(_configuration.EmailSenderCredentials.EmailSender,
                _configuration.EmailSenderCredentials.Password);

            _logger.Info("Sending {EmailSubject}...", mail.Subject);
            await _smtpClient.SendAsync(mail);

            _logger.Info("Stopping Email Provider and closing connection...");
            await _smtpClient.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            _logger.Warn(e, "Error thrown on sending email: {EmailSubject}", mail.Subject);
            throw;
        }
    }

    public Task<EmailSettings> GetEmailSettings() => Task.FromResult(_configuration.ProviderSettings);
}