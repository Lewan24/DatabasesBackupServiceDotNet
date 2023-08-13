using Application.Data.Interfaces;
using Core.Entities.Models;
using Core.Exceptions;
using Core.StaticClassess;
using Newtonsoft.Json;
using NLog;

namespace Application.Data.Services;

public class ApplicationService : IApplicationService
{
    private readonly Logger _logger;
    private readonly IDbBackupService _backupService;
    private readonly IEmailProviderService _emailProviderService;

    public ApplicationService(Logger logger, IDbBackupService backupService, IEmailProviderService emailProviderService)
    {
        _logger = logger.Factory.GetLogger(nameof(ApplicationService));
        _backupService = backupService;
        _emailProviderService = emailProviderService;
    }
    
    public async Task RunService()
    {
        _logger.Debug("Service started");

        try
        {
            _logger.Info("Getting needed configurations...");
            var dbConfigurationsList = await ReadDatabaseConfigurations();
            
            await _backupService.RunService(dbConfigurationsList);
        }
        catch (Exception e)
        {
            _logger.Warn(e);
            // TODO: Check and test different types of emails like errors, each db success email, or each failure
            if ((await _emailProviderService.GetEmailSettings()).SendEmailOnOtherFailures)
                await _emailProviderService.PrepareAndSendEmail(new MailModel("There was a problem in the backup service",
                    PrepareEmailMessageBody.PrepareErrorReport($"<b>Error occurs while reading databases configurations</b><br><i><b>Error message: </b>{e.Message}</i>")));
        }
        finally
        {
            await StopService();
        }
    }

    private async Task<List<DatabaseConfigModel>> ReadDatabaseConfigurations()
    {
        try
        {
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "ConfigurationFiles", "databasesConfigurations.json");
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            
            var configs = JsonConvert.DeserializeObject<List<DatabaseConfigModel>>(jsonContent)!;
            
            return configs;
        }
        catch (Exception e)
        {
            _logger.Warn(e, "Error on reading config file");
            throw;
        }
    }
    
    private async Task StopService()
    {
        var madeBackupsCount = await _backupService.GetBackupsCounter();
        _logger.Info("{ServiceName} successfully stopped with {BackupsCount} made backups", nameof(ApplicationService), madeBackupsCount);

        await _emailProviderService.PrepareAndSendEmail(new MailModel("Backups Service Statistics", 
            PrepareEmailMessageBody.PrepareStatisticsReport("Backups finish time: <b><i>{DateTime.Now:t}</i></b><br>Number of Successfully made backups: <b><i>{madeBackupsCount}</i></b>")));

        _logger.Debug("Service stopped");
    }
}