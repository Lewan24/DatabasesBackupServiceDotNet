using Application.Data.Interfaces;
using Core.Entities.Models;
using Core.StaticClasses;
using Newtonsoft.Json;
using NLog;

namespace Application.Data.Services;

public class ApplicationService(
    Logger logger,
    IDbBackupService backupService,
    IEmailProviderService emailProviderService)
    : IApplicationService
{
    private readonly Logger _logger = logger.Factory.GetLogger(nameof(ApplicationService));

    public async Task RunService()
    {
        _logger.Debug("Service started");

        try
        {
            _logger.Info("Getting needed configurations...");
            var dbConfigurationsList = await ReadDatabaseConfigurations();

            await backupService.RunService(dbConfigurationsList);
        }
        catch (Exception e)
        {
            _logger.Warn(e);

            if ((await emailProviderService.GetEmailSettings()).SendEmailOnOtherFailures)
                await emailProviderService.PrepareAndSendEmail(new MailModel(
                    "There was a problem in the backup service",
                    EmailMessageBody.PrepareErrorReport(
                        $"<b><span style='color: red'>Error occurs while reading databases configurations</span></b><br><i><b>Error message: </b><span style='color: red'>{e.Message}</span></i>")));
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
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "ConfigurationFiles",
                "databasesConfigurations.json");
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
        var madeBackupsCount = await backupService.GetBackupsCounter();
        _logger.Info("{ServiceName} successfully stopped with {BackupsCount} made backups", nameof(ApplicationService),
            madeBackupsCount);

        await emailProviderService.PrepareAndSendEmail(new MailModel("Backups Service Statistics",
            EmailMessageBody.PrepareStatisticsReport(
                $"Backups finish time: <b><i>{DateTime.Now:t}</i></b><br>Number of Successfully made backups: <b><i><span style='color: green'>{madeBackupsCount}</span></i></b>")));

        _logger.Debug("Service stopped");
    }
}