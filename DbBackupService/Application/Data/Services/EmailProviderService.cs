using Application.Data.Interfaces;
using Core.Entities.Models;
using NLog;

namespace Application.Data.Services;

public class EmailProviderService : IEmailProviderService
{
    private readonly EmailProviderConfigurationModel _configuration;
    private readonly Logger _logger;

    public EmailProviderService (EmailProviderConfigurationModel configuration, Logger logger)
    {
        _configuration = configuration;
        _logger = logger.Factory.GetLogger(nameof(EmailProviderService));
    }

    public Task SendMail()
    {
        throw new NotImplementedException();
    }
}